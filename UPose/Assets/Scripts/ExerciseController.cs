
using UnityEngine;

public class ExerciseController : MonoBehaviour
{
    public enum Exercise
    {
        None,
        ArmRaise,
        LeftLunge,
        RightLunge
    }

    public enum ExerciseState
    {
        Idle,
        Playing,
        Paused,
        Complete
    }

    [Header("Exercise Settings")]
    public float repetitionDuration = 4f;
    public int targetRepetitions = 10;
    public float armRaiseAngle = 90f;

    // ref lunge motion controller script
    public LungeMotionController lungeController;

    [Header("Lunge Settings")]
    public float frontHipAngle = 70f;
    public float backHipAngle = -30f;
    public float frontKneeAngle = -75f;
    public float backKneeAngle = -30f;
    public float hipDrop = 0.15f;

    [Header("Current Status")]
    public Exercise currentExercise = Exercise.None;
    public ExerciseState state = ExerciseState.Idle;
    public int completedRepetitions = 0;

    // Arm bones
    private Transform leftArm;
    private Transform rightArm;

    // Upper leg bones (hip joints)
    private Transform leftUpLeg;
    private Transform rightUpLeg;

    // Lower leg bones (knee joints)
    private Transform leftLeg;
    private Transform rightLeg;

    // Foot and grounding
    private Transform leftFoot;
    private Transform rightFoot;

    private float groundHeight;

    // Pelvis
    private Transform hips;

    // Original rotations
    private Quaternion leftArmStart;
    private Quaternion rightArmStart;
    private Quaternion leftUpLegStart;
    private Quaternion rightUpLegStart;
    private Quaternion leftLegStart;
    private Quaternion rightLegStart;

    private Vector3 hipsStartPosition;

    private bool initialized = false;
    private float elapsed = 0f;
    private float nextBoneSearch = 0f;

    void Update()
    {
        if (!initialized && Time.time >= nextBoneSearch)
        {
            nextBoneSearch = Time.time + 0.5f;
            FindBones();
        }

        HandleKeyboardInput();
    }

    bool Matches(string boneName, string target)
    {
        string name = boneName.ToLowerInvariant();
        return name == target || name.EndsWith(":" + target);
    }

    void FindBones()
    {
        Transform[] bones =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform bone in bones)
        {
            if (bone == null) continue;

            if (Matches(bone.name, "leftarm") ||
                Matches(bone.name, "leftupperarm"))
                leftArm = bone;

            if (Matches(bone.name, "rightarm") ||
                Matches(bone.name, "rightupperarm"))
                rightArm = bone;

            if (Matches(bone.name, "leftupleg") ||
                Matches(bone.name, "leftupperleg"))
                leftUpLeg = bone;

            if (Matches(bone.name, "rightupleg") ||
                Matches(bone.name, "rightupperleg"))
                rightUpLeg = bone;

            if (Matches(bone.name, "leftleg"))
                leftLeg = bone;

            if (Matches(bone.name, "rightleg"))
                rightLeg = bone;

            if (Matches(bone.name, "leftfoot"))
                leftFoot = bone;

            if (Matches(bone.name, "rightfoot"))
                rightFoot = bone;

            if (Matches(bone.name, "hips"))
                hips = bone;
        }

        if (leftArm == null || rightArm == null ||
            leftUpLeg == null || rightUpLeg == null ||
            leftLeg == null || rightLeg == null ||
            hips == null)
        {
            return;
        }

        leftArmStart = leftArm.localRotation;
        rightArmStart = rightArm.localRotation;

        leftUpLegStart = leftUpLeg.localRotation;
        rightUpLegStart = rightUpLeg.localRotation;

        leftLegStart = leftLeg.localRotation;
        rightLegStart = rightLeg.localRotation;

        groundHeight = Mathf.Min(
            leftFoot.position.y,
            rightFoot.position.y
        );

        hipsStartPosition = hips.localPosition;

        initialized = true;

        Debug.Log("ExerciseController: All bones ready!");
    }

    public void StartExercise(string exerciseName)
    {
        if (!initialized)
        {
            Debug.LogWarning("Avatar not loaded yet.");
            return;
        }

        Exercise selected;

        switch (exerciseName.ToLowerInvariant())
        {
            case "arm_raise":
                selected = Exercise.ArmRaise;
                break;

            case "left_lunge":
                selected = Exercise.LeftLunge;
                break;

            case "right_lunge":
                selected = Exercise.RightLunge;
                break;

            default:
                Debug.LogWarning("Unknown exercise: " + exerciseName);
                return;
        }

        ResetPose();

        if (selected == Exercise.LeftLunge)
            lungeController.StartLunge(true);
        else if (selected == Exercise.RightLunge)
            lungeController.StartLunge(false);

        currentExercise = selected;
        completedRepetitions = 0;
        elapsed = 0f;
        state = ExerciseState.Playing;

        Debug.Log("Starting exercise: " + exerciseName);
    }

    public void StopExercise()
    {
        ResetPose();

        currentExercise = Exercise.None;
        state = ExerciseState.Idle;
        completedRepetitions = 0;
        elapsed = 0f;

        Debug.Log("Exercise stopped");
    }

    public void PauseExercise()
    {
        if (state != ExerciseState.Playing)
            return;

        state = ExerciseState.Paused;
        Debug.Log("Exercise paused");
    }

    public void ResumeExercise()
    {
        if (state != ExerciseState.Paused)
            return;

        state = ExerciseState.Playing;
        Debug.Log("Exercise resumed");
    }

    void LateUpdate()
    {
        if (!initialized)
            return;

        if (state != ExerciseState.Playing &&
            state != ExerciseState.Paused)
            return;

        float duration = Mathf.Max(0.1f, repetitionDuration);

        if (state == ExerciseState.Playing)
        {
            elapsed += Time.deltaTime;

            int newCount = Mathf.FloorToInt(elapsed / duration);

            if (newCount > completedRepetitions)
            {
                completedRepetitions = newCount;
                Debug.Log("Completed rep: " + completedRepetitions);
            }

            if (targetRepetitions > 0 &&
                completedRepetitions >= targetRepetitions)
            {
                ResetPose();
                state = ExerciseState.Complete;

                Debug.Log("Exercise complete!");
                return;
            }
        }

        // Calculate the same pose even when paused.
        float progress = (elapsed % duration) / duration;

        switch (currentExercise)
        {
            case Exercise.ArmRaise:
                AnimateArmRaise(progress);
                break;

            case Exercise.LeftLunge:
                lungeController.StartLunge(true);
                break;

            case Exercise.RightLunge:
                lungeController.StartLunge(false);
                break;
        }
    }

    void AnimateArmRaise(float progress)
    {
        float angle = armRaiseAngle *
            Mathf.Sin(progress * Mathf.PI);

        leftArm.localRotation =
            leftArmStart * Quaternion.Euler(0f, 0f, angle);

        rightArm.localRotation =
            rightArmStart * Quaternion.Euler(0f, 0f, -angle);
    }

    void AnimateLunge(float progress, bool leftIsFront)
    {
        // Smoothly lower into and rise out of a lunge.
        float amount = Mathf.Sin(progress * Mathf.PI);
        amount = Mathf.Clamp01(amount);

        // Select leading and trailing leg bones.
        Transform frontThigh = leftIsFront
            ? leftUpLeg : rightUpLeg;

        Transform backThigh = leftIsFront
            ? rightUpLeg : leftUpLeg;

        Transform frontShin = leftIsFront
            ? leftLeg : rightLeg;

        Transform backShin = leftIsFront
            ? rightLeg : leftLeg;

        Quaternion frontThighStart = leftIsFront
            ? leftUpLegStart : rightUpLegStart;

        Quaternion backThighStart = leftIsFront
            ? rightUpLegStart : leftUpLegStart;

        Quaternion frontShinStart = leftIsFront
            ? leftLegStart : rightLegStart;

        Quaternion backShinStart = leftIsFront
            ? rightLegStart : leftLegStart;

        // 1. Move the leading thigh forward.
        frontThigh.localRotation =
            frontThighStart * Quaternion.Euler(
                frontHipAngle * amount, 0f, 0f);

        // 2. Extend the trailing thigh backward.
        backThigh.localRotation =
            backThighStart * Quaternion.Euler(
                backHipAngle * amount, 0f, 0f);

        // 3. Bend the leading knee.
        frontShin.localRotation =
            frontShinStart * Quaternion.Euler(
                frontKneeAngle * amount, 0f, 0f);

        // 4. Bend the trailing knee.
        backShin.localRotation =
            backShinStart * Quaternion.Euler(
                backKneeAngle * amount, 0f, 0f);

        float lowestFoot = Mathf.Min(
            leftFoot.position.y,
            rightFoot.position.y
        );

        float correction = groundHeight - lowestFoot;

        hips.position += Vector3.up * correction;
        // // 5. Lower the pelvis.
        // hips.localPosition =
        //     hipsStartPosition +
        //     Vector3.down * (hipDrop * amount);
    }

    void ResetPose()
    {
        if (!initialized)
            return;

        leftArm.localRotation = leftArmStart;
        rightArm.localRotation = rightArmStart;

        leftUpLeg.localRotation = leftUpLegStart;
        rightUpLeg.localRotation = rightUpLegStart;

        leftLeg.localRotation = leftLegStart;
        rightLeg.localRotation = rightLegStart;

        hips.localPosition = hipsStartPosition;
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartExercise("arm_raise");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            StartExercise("left_lunge");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            StartExercise("right_lunge");

        if (Input.GetKeyDown(KeyCode.S))
            StopExercise();

        if (Input.GetKeyDown(KeyCode.P))
            PauseExercise();

        if (Input.GetKeyDown(KeyCode.R))
            ResumeExercise();
    }
}
