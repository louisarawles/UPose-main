
import json
import socket
import time
from pathlib import Path
# added for smoothing out the recordings
import numpy as np
from scipy.spatial.transform import Rotation, Slerp

# Same destination as UPose's live MediaPipe script
UNITY_IP = "127.0.0.1"
UNITY_PORT = 52733

RECORDINGS_DIR = Path(__file__).resolve().parent / "ExerciseRecordings"


def parse_pose(data):
    """Convert an UPose mprot string to joint data."""
    joints = {}

    for line in data.strip().splitlines():
        if not line or line == "mprot":
            continue

        parts = line.split("|")
        if len(parts) != 6:
            continue

        joint_id = int(parts[0])
        quat = np.array(
            [float(x) for x in parts[1:5]]
        )
        visibility = float(parts[5])

        joints[joint_id] = (quat, visibility)

    return joints


def interpolate_pose(pose_a, pose_b, alpha):
    """Interpolate joint rotations using SLERP."""
    joints_a = parse_pose(pose_a)
    joints_b = parse_pose(pose_b)

    lines = ["mprot"]

    for joint_id in sorted(joints_a):
        qa, va = joints_a[joint_id]

        if joint_id not in joints_b:
            raise ValueError("Mismatched joint IDs")

        qb, vb = joints_b[joint_id]

        rotations = Rotation.from_quat([qa, qb])
        slerp = Slerp([0, 1], rotations)
        q = slerp([alpha]).as_quat()[0]

        visibility = va + alpha * (vb - va)

        lines.append(
            f"{joint_id}|{q[0]}|{q[1]}|"
            f"{q[2]}|{q[3]}|{visibility}"
        )

    return "\n".join(lines) + "\n"

def play_exercise(exercise_name):
    file_path = RECORDINGS_DIR / f"{exercise_name}.json"

    if not file_path.exists():
        print(f"Recording not found: {file_path}")
        return

    with open(file_path, "r") as f:
        recording = json.load(f)

    frames = recording["frames"]

    if not frames:
        print("Recording contains no frames.")
        return

    
    target_fps = 60
    first_time = float(frames[0]["time"])
    duration = float(frames[-1]["time"]) - first_time

    frame_index = 0

    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sock:
        start_time = time.monotonic()
        output_count = int(np.ceil(duration * target_fps))

        for i in range(output_count + 1):
            t = min(i / target_fps, duration)
            recorded_time = first_time + t

            while (
                frame_index + 2 < len(frames)
                and frames[frame_index + 1]["time"] < recorded_time
            ):
                frame_index += 1

            a = frames[frame_index]
            b = frames[min(frame_index + 1, len(frames) - 1)]

            t0 = float(a["time"])
            t1 = float(b["time"])

            alpha = (
                (recorded_time - t0) / (t1 - t0)
                if t1 > t0 else 0.0
            )
            alpha = max(0.0, min(1.0, alpha))

            interpolated_pose = interpolate_pose(
                a["pose"], b["pose"], alpha
            )

            delay = start_time + t - time.monotonic()
            if delay > 0:
                time.sleep(delay)

            message = (interpolated_pose + "<EOM>").encode("utf-8")
            sock.sendto(message, (UNITY_IP, UNITY_PORT))

    print(f"Finished playing {exercise_name}")


if __name__ == "__main__":
    while True:
        command = input(
            "\nEnter exercise (squat, forward_lunge_left, forward_lunge_right) or 'quit': "
        ).strip().lower()

        if command == "quit":
            break

        if command:
            play_exercise(command)
