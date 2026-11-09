# UPose with Exercise Command Feature
Last Update: 11.9.2026

## Overview
The UPose system was modified to allow for exercise playback within the Unity game scene. Modifications were made to the existing MediaPipe file, and files for recording playback and storage were created to support the new capability.

## Added Features
- Record human motion within MediaPipe
- Playback human motion recordings with the 3D avatar

## Updates

### `run_mediapipe.py`
#### Global Variables for Runtime Configuration

- `STREAM_TO_UNITY`: boolean global variable; indicates whether MediaPipe should attempt to connect to the UPose Unity server or not
    - Important for performance; if you're creating a recording without running the Unity game, then failed attempts to run the server will create lag in the recording
- `exercise_name`: string value that would become the name of the json file if a recording was made while running MediaPipe
    - Allows for specifying the exercise in terminal during recording playback 
    - Note: current exercise recording names are listed in the command input prompt in `"__main__"` in `play_recording.py`, so additional motions implemented should also be added to this prompt

#### Added Content
- `save_recording`: function to save a recorded frame in a json file that can be sent directly to the Unity server
    - Variables to store recording information, including appending frames to the list of recorded frames
    - Call to function occurs at the end of the file before releasing the video capture (aka, closing the webcam) and destroying the webcam window
    - *Note*: To save an exercise recording to a new file, you will have to stop MediaPipe and set the exercise name to your new desired title.
- Added conditionals to check if the system is streaming to unity or not and handlers

#### How to Run - Live Stream to Unity ONLY
Follow the instructions in the UPose `README.md`.

#### How to Run - Recording + Live Stream to Unity
1. Open `run_mediapipe.py` and make sure that `STREAM_TO_UNITY` is set to True, and the exercise name is set to the correct title for your motion.
2. Follow the instructions in the UPose `README.md` so that `run_mediapipe.py` can connect to the Unity server.
3. Click on the window with the live camera feedback. 
4. Click 'R' on your keyboard -- a message in terminal should appear indicating that the recording has started.
5. Perform your motion.
6. Click 'S' on your keyboard to stop the recording -- a message should appear in the terminal indicating that the recording has stopped.

#### How to Run - Recording ONLY
1. Open `run_mediapipe.py` and make sure that `STREAM_TO_UNITY` is set to False, and the exercise name is set to the correct title for your motion.
2. In a terminal with the activated `mediapipe` venv, run `python run_mediapipe.py` (or `python3` depending on your system).
3. Click on the window with the live camera feedback. 
4. Click 'R' on your keyboard -- a message in terminal should appear indicating that the recording has started.
5. Perform your motion.
6. Click 'S' on your keyboard to stop the recording -- a message should appear in the terminal indicating that the recording has stopped.

-------

### `play_recording.py`
#### Overview
- Sends data from the recordings directory to UPose's live Unity server
- Interpolates joint rotations to smooth movement between frames
- Sends interpolated frames for a given exercise name to the server address at a target of 60 frames per second
- Includes prompt for user to enter an exercise or quit the program in terminal

#### How to Run
Before running `play_recording.py`, make sure you have the UPose game running in Unity. See the UPose `README.md` for more details.

1. In a terminal shell with the mediapipe venv activated (see UPose's original `README.md`), run `python play_recording.py` (or `python3` depending on your system).
2. The command prompt will appear in the terminal, providing options for available exercises that can be performed by the avatar. Type in one of the options in terminal and hit enter. 
3. Immediately click on the window with the avatar in the exercise scene in the Unity game, and it will perform the recording.

-------

### ExerciseRecordings
- Directory containing the motion recordings as json files
- Contents modified in the backend when a recording is made while running MediaPipe
