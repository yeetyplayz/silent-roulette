using UnityEngine;

/// <summary>
/// Adds a subtle hand-held sway effect to a camera.
/// Attach directly to the overhead peek camera.
/// </summary>
public class CameraSway : MonoBehaviour
{
    [Header("Position Sway")]
    public float positionSwayAmount = 0.002f;
    public float positionSwaySpeed = 0.8f;

    [Header("Rotation Sway")]
    public float rotationSwayAmount = 0.3f;
    public float rotationSwaySpeed = 0.6f;

    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

    void Start()
    {
        _originalLocalPosition = transform.localPosition;
        _originalLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float time = Time.time;

        // Gentle position drift using offset sin waves on each axis
        float x = Mathf.Sin(time * positionSwaySpeed * 1.1f) * positionSwayAmount;
        float y = Mathf.Sin(time * positionSwaySpeed * 0.9f) * positionSwayAmount * 0.5f;
        float z = Mathf.Sin(time * positionSwaySpeed * 0.7f) * positionSwayAmount * 0.3f;

        transform.localPosition = _originalLocalPosition + new Vector3(x, y, z);

        // Gentle rotation drift
        float pitch = Mathf.Sin(time * rotationSwaySpeed * 1.0f) * rotationSwayAmount;
        float yaw = Mathf.Sin(time * rotationSwaySpeed * 0.8f) * rotationSwayAmount * 0.5f;
        float roll = Mathf.Sin(time * rotationSwaySpeed * 1.2f) * rotationSwayAmount * 0.3f;

        transform.localRotation = _originalLocalRotation * Quaternion.Euler(pitch, yaw, roll);
    }
}