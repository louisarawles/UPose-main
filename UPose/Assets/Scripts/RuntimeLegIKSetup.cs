
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RuntimeLegIKSetup : MonoBehaviour
{
    [Header("IK Components")]
    public RigBuilder rigBuilder;
    public TwoBoneIKConstraint leftLegIK;

    [Header("Targets")]
    public Transform leftFootTarget;
    public Transform leftKneeHint;

    [Header("Settings")]
    public float kneeHintOffset = 0.2f;

    private bool initialized = false;

    void Update()
    {
        if (initialized)
            return;

        Transform leftThigh = FindBone("LeftUpLeg");
        Transform leftKnee = FindBone("LeftLeg");
        Transform leftFoot = FindBone("LeftFoot");

        // Wait for UPose to finish loading
        if (leftThigh == null ||
            leftKnee == null ||
            leftFoot == null)
            return;

        if (rigBuilder == null ||
            leftLegIK == null ||
            leftFootTarget == null ||
            leftKneeHint == null)
        {
            Debug.LogError("IK references missing!");
            enabled = false;
            return;
        }

        // Match target to the loaded foot
        leftFootTarget.position = leftFoot.position;
        leftFootTarget.rotation = leftFoot.rotation;

        // Position hint in front of knee
        leftKneeHint.position =
            leftKnee.position +
            transform.forward * kneeHintOffset;

        // Configure the Two Bone IK constraint
        var data = leftLegIK.data;

        data.root = leftThigh;
        data.mid = leftKnee;
        data.tip = leftFoot;

        data.target = leftFootTarget;
        data.hint = leftKneeHint;

        data.targetPositionWeight = 1f;
        data.targetRotationWeight = 0f;
        data.hintWeight = 1f;

        data.maintainTargetPositionOffset = false;
        data.maintainTargetRotationOffset = false;

        // Assign the modified constraint data
        leftLegIK.data = data;
        leftLegIK.weight = 1f;

        // Rebuild after assigning runtime bones
        rigBuilder.Clear();
        rigBuilder.Build();

        initialized = true;

        Debug.Log("Left leg IK initialized!");
    }

    Transform FindBone(string boneName)
    {
        foreach (Transform bone in
            GetComponentsInChildren<Transform>(true))
        {
            string name = bone.name.ToLowerInvariant();
            string target = boneName.ToLowerInvariant();

            if (name == target ||
                name.EndsWith(":" + target))
                return bone;
        }

        return null;
    }
}
