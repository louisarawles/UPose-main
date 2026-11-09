
using UnityEngine;

public class LungeMotionController : MonoBehaviour
{
    [Header("References")]
    public Transform avatarRoot;
    public Transform hips;
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    [Header("Motion Settings")]
    public float repetitionDuration = 4f;
    public float stanceDistance = 0.35f;
    public float hipDrop = 0.20f;

    private Vector3 initialHipsPosition;
    private Vector3 initialLeftFootPosition;
    private Vector3 initialRightFootPosition;

    private float elapsed = 0f;
    private bool initialized = false;
    private bool playing = false;
    private bool paused = false;
    private bool leftIsFront = true;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (avatarRoot == null || hips == null ||
            leftFootTarget == null ||
            rightFootTarget == null)
        {
            Debug.LogWarning("Lunge references not ready.");
            return;
        }

        initialHipsPosition = hips.position;
        initialLeftFootPosition = leftFootTarget.position;
        initialRightFootPosition = rightFootTarget.position;

        initialized = true;
    }

    public void StartLunge(bool leftFront)
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        leftIsFront = leftFront;
        elapsed = 0f;
        playing = true;
        paused = false;

        // Set initial split stance.
        Vector3 forward = avatarRoot.forward;

        leftFootTarget.position =
            initialLeftFootPosition +
            forward * (leftIsFront
                ? stanceDistance : -stanceDistance);

        rightFootTarget.position =
            initialRightFootPosition +
            forward * (leftIsFront
                ? -stanceDistance : stanceDistance);
    }

    public void PauseLunge()
    {
        paused = true;
    }

    public void ResumeLunge()
    {
        paused = false;
    }

    public void StopLunge()
    {
        playing = false;
        paused = false;
        elapsed = 0f;

        if (!initialized) return;

        hips.position = initialHipsPosition;
        leftFootTarget.position = initialLeftFootPosition;
        rightFootTarget.position = initialRightFootPosition;
    }

    void Update()
    {
        if (!initialized || !playing || paused)
            return;

        elapsed += Time.deltaTime;
    }

    void LateUpdate()
    {
        if (!initialized || !playing)
            return;

        float duration = Mathf.Max(
            0.1f, repetitionDuration
        );

        float progress =
            (elapsed % duration) / duration;

        // Smoothly move down and back up.
        float amount = Mathf.Sin(
            progress * Mathf.PI
        );

        hips.position =
            initialHipsPosition -
            Vector3.up * hipDrop * amount;
    }
}
