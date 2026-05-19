using UnityEngine;

public class TablePeekCamera : MonoBehaviour
{
    [Header("Peek Settings")]
    public float peekHeight = 4f;
    [Tooltip("Shift the peek position to center the table. Adjust X/Y/Z in play mode until it looks right.")]
    public Vector3 peekOffset = Vector3.zero;
    public float transitionSpeed = 5f;
    public KeyCode peekKey = KeyCode.LeftAlt;

    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private bool _isPeeking = false;

    private BlackjackCameraController _fpsController;

    void Start()
    {
        _fpsController = GetComponent<BlackjackCameraController>();
        _savedPosition = transform.position;
        _savedRotation = transform.rotation;
        _targetPosition = transform.position;
        _targetRotation = transform.rotation;
    }

    void Update()
    {
        bool holdingPeek = Input.GetKey(peekKey);

        if (holdingPeek && !_isPeeking)
        {
            _isPeeking = true;
            _savedPosition = transform.position;
            _savedRotation = transform.rotation;
            _targetPosition = _savedPosition + Vector3.up * peekHeight + peekOffset;
            _targetRotation = Quaternion.Euler(0f, 0f, 90f);
            if (_fpsController != null) _fpsController.enabled = false;
        }

        if (!holdingPeek && _isPeeking)
        {
            _isPeeking = false;
            _targetPosition = _savedPosition;
            _targetRotation = _savedRotation;
        }

        transform.position = Vector3.Lerp(transform.position, _targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, transitionSpeed * Time.deltaTime);

        if (!_isPeeking && _fpsController != null && !_fpsController.enabled)
        {
            if (Vector3.Distance(transform.position, _savedPosition) < 0.02f &&
                Quaternion.Angle(transform.rotation, _savedRotation) < 1f)
            {
                transform.position = _savedPosition;
                transform.rotation = _savedRotation;
                _fpsController.enabled = true;
            }
        }
    }
}