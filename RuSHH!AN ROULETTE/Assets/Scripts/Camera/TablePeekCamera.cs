using UnityEngine;

public class TablePeekCamera : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("Your main player camera — stays untouched, just gets enabled/disabled.")]
    public Camera playerCamera;

    [Tooltip("A second camera at root level in the Hierarchy positioned exactly where you want the overhead view.")]
    public Camera overheadCamera;

    [Header("Peek Settings")]
    public KeyCode peekKey = KeyCode.LeftAlt;
    public float transitionSpeed = 8f;

    public bool IsBettingPhase = false;

    private BlackjackCameraController _fpsController;
    private bool _isPeeking = false;
    private float _blend = 0f;

    void Start()
    {
        _fpsController = playerCamera != null
            ? playerCamera.GetComponent<BlackjackCameraController>()
            : GetComponent<BlackjackCameraController>();

        // Overhead camera starts disabled
        if (overheadCamera != null)
            overheadCamera.enabled = false;

        if (playerCamera != null)
            playerCamera.enabled = true;
    }

    void Update()
    {
        if (IsBettingPhase) return;

        bool holdingPeek = Input.GetKey(peekKey);

        if (holdingPeek && !_isPeeking)
        {
            _isPeeking = true;
            if (_fpsController != null) _fpsController.SetInputLocked(true);
        }

        if (!holdingPeek && _isPeeking)
        {
            _isPeeking = false;
        }

        float targetBlend = _isPeeking ? 1f : 0f;
        _blend = Mathf.MoveTowards(_blend, targetBlend, transitionSpeed * Time.deltaTime);

        // Swap cameras based on blend threshold
        if (playerCamera != null) playerCamera.enabled = _blend < 0.5f;
        if (overheadCamera != null) overheadCamera.enabled = _blend >= 0.5f;

        // Re-enable input once fully back
        if (!_isPeeking && _fpsController != null && _blend <= 0f)
            _fpsController.SetInputLocked(false);
    }
}