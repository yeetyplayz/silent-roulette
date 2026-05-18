using UnityEngine;

/// <summary>
/// First-person camera controller for a seated player at a blackjack table.
/// Attach to the Camera (or a Camera parent object).
///
/// Controls:
///   Mouse        — Free look (constrained to seated range)
///   1 / 2 / 3   — Snap to look at player seat 1, 2, 3
///   Tab          — Cycle through seats
///   Escape       — Return to forward (dealer) view
/// </summary>
public class BlackjackCameraController : MonoBehaviour
{
    [Header("Mouse Look")]
    [Tooltip("How fast the camera rotates with the mouse.")]
    public float mouseSensitivity = 150f;

    [Tooltip("Max degrees you can look up or down (seated, so keep this small).")]
    public float pitchClamp = 25f;

    [Tooltip("Max degrees you can look left or right from centre.")]
    public float yawClamp = 110f;

    [Header("Snap Settings")]
    [Tooltip("How fast the camera snaps to a target seat (degrees per second).")]
    public float snapSpeed = 180f;

    [Tooltip("Angle threshold (degrees) at which a snap is considered 'arrived'.")]
    public float snapArrivalThreshold = 0.5f;

    // The neutral forward rotation (set once at Start so we always know where "dealer" is)
    private Quaternion _neutralRotation;

    // Current Euler angles we're tracking internally
    private float _yaw;    // left-right
    private float _pitch;  // up-down

    // Snap state
    private bool _isSnapping = false;
    private Quaternion _snapTarget;

    // Reference to the seat manager (optional — only needed for key bindings)
    private SeatSnapManager _snapManager;

    void Start()
    {
        _neutralRotation = transform.rotation;

        // Decompose the neutral rotation into yaw/pitch so mouse look starts centred
        _yaw = transform.eulerAngles.y;
        _pitch = 0f;

        _snapManager = GetComponent<SeatSnapManager>();
        if (_snapManager == null)
            _snapManager = GetComponentInParent<SeatSnapManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleKeyBindings();

        if (_isSnapping)
            DoSnap();
        else
            DoMouseLook();
    }

    // -----------------------------------------------------------------------
    //  Mouse look
    // -----------------------------------------------------------------------
    void DoMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;

        // Clamp pitch (up/down) — seated, so we barely tilt
        _pitch = Mathf.Clamp(_pitch, -pitchClamp, pitchClamp);

        // Clamp yaw relative to neutral forward
        float neutralYaw = _neutralRotation.eulerAngles.y;
        float delta = Mathf.DeltaAngle(neutralYaw, _yaw);
        if (Mathf.Abs(delta) > yawClamp)
            _yaw = neutralYaw + Mathf.Sign(delta) * yawClamp;

        ApplyRotation(_yaw, _pitch);
    }

    // -----------------------------------------------------------------------
    //  Smooth snap to a target rotation
    // -----------------------------------------------------------------------
    void DoSnap()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            _snapTarget,
            snapSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, _snapTarget) < snapArrivalThreshold)
        {
            transform.rotation = _snapTarget;
            _isSnapping = false;

            // Sync internal yaw/pitch so mouse look picks up from the snapped position
            _yaw = transform.eulerAngles.y;
            _pitch = WrapAngle(transform.eulerAngles.x);
        }
    }

    // -----------------------------------------------------------------------
    //  Public API — called by SeatSnapManager or game logic
    // -----------------------------------------------------------------------

    /// <summary>Smoothly rotate to look at a world-space position.</summary>
    public void SnapToPosition(Vector3 worldTarget)
    {
        Vector3 dir = (worldTarget - transform.position).normalized;
        if (dir == Vector3.zero) return;
        _snapTarget = Quaternion.LookRotation(dir);
        _isSnapping = true;
    }

    /// <summary>Return to the neutral dealer-facing view.</summary>
    public void SnapToNeutral()
    {
        _snapTarget = _neutralRotation;
        _isSnapping = true;
    }

    /// <summary>Cancel any ongoing snap and resume mouse look immediately.</summary>
    public void CancelSnap()
    {
        _isSnapping = false;
        _yaw = transform.eulerAngles.y;
        _pitch = WrapAngle(transform.eulerAngles.x);
    }

    // -----------------------------------------------------------------------
    //  Key bindings
    // -----------------------------------------------------------------------
    void HandleKeyBindings()
    {
        // Escape → look at dealer (neutral)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SnapToNeutral();
            return;
        }

        // Tab → cycle seats
        if (Input.GetKeyDown(KeyCode.Tab) && _snapManager != null)
        {
            _snapManager.CycleToNextSeat(this);
            return;
        }

        // 1 / 2 / 3 → specific seats
        if (_snapManager != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) _snapManager.SnapToSeat(0, this);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _snapManager.SnapToSeat(1, this);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _snapManager.SnapToSeat(2, this);
        }
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------
    void ApplyRotation(float yaw, float pitch)
    {
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// Converts Unity's 0-360 euler X back to -180..180 so pitch clamp works correctly.
    float WrapAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}