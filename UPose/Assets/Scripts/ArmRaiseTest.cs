
using UnityEngine;

public class ArmRaiseTest : MonoBehaviour
{
    private Transform leftArm;
    private Transform rightArm;

    private Quaternion leftStart;
    private Quaternion rightStart;

    private bool initialized = false;
    private float elapsed = 0f;

    void Update()
    {
        if (initialized) return;

        // Wait for UPose to load the avatar
        Transform[] bones =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform bone in bones)
        {
            if (bone == null) continue;

            string name = bone.name.ToLowerInvariant();

            if (name == "leftarm" ||
                name == "leftupperarm" ||
                name.EndsWith(":leftarm"))
                leftArm = bone;

            if (name == "rightarm" ||
                name == "rightupperarm" ||
                name.EndsWith(":rightarm"))
                rightArm = bone;
        }

        if (leftArm != null && rightArm != null)
        {
            leftStart = leftArm.localRotation;
            rightStart = rightArm.localRotation;

            initialized = true;

            Debug.Log("Arm animation initialized!");
        }
    }

    void LateUpdate()
    {
        if (!initialized) return;

        elapsed += Time.deltaTime;

        // Full exercise cycle every 4 seconds
        float progress = (elapsed % 4f) / 4f;

        // 0 -> 90 -> 0 degrees
        float angle = 90f * Mathf.Sin(progress * Mathf.PI);

        leftArm.localRotation =
            leftStart * Quaternion.Euler(0f, 0f, angle);

        rightArm.localRotation =
            rightStart * Quaternion.Euler(0f, 0f, -angle);
    }
}
