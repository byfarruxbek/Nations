using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls 3D Globe rotation via mouse drag, mobile touch drag, and inertia.
/// Also provides smooth auto-spin when idle.
/// </summary>
public class GlobeRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Sensitivity of touch and mouse dragging.")]
    public float rotationSpeed = 0.4f;
    [Tooltip("Inertia damping factor when dragging stops.")]
    public float inertiaDamping = 0.93f;
    [Tooltip("Subtle auto-rotation speed when idle.")]
    public float autoRotationSpeed = 8.0f;

    [Header("Focus Animation")]
    public float focusSmoothTime = 0.5f;

    private Vector2 rotationVelocity;
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private bool isFocusing = false;
    private Quaternion targetRotation;

    private void Update()
    {
        // Check for UI block so dragging doesn't interfere when touching UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (!isDragging) return;
        }

        HandleInput();

        if (isFocusing)
        {
            // Smoothly rotate globe towards selected country
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / focusSmoothTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                isFocusing = false;
            }
        }
        else if (isDragging)
        {
            // Apply drag rotation
            transform.Rotate(Vector3.up, -rotationVelocity.x, Space.World);
            transform.Rotate(Vector3.right, rotationVelocity.y, Space.World);
        }
        else
        {
            // Apply inertia deceleration
            if (rotationVelocity.magnitude > 0.05f)
            {
                transform.Rotate(Vector3.up, -rotationVelocity.x, Space.World);
                transform.Rotate(Vector3.right, rotationVelocity.y, Space.World);
                rotationVelocity *= inertiaDamping;
            }
            else
            {
                // Idle continuous slow rotation
                transform.Rotate(Vector3.up, -autoRotationSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    private void HandleInput()
    {
        // Mouse / Touch Down
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                isFocusing = false;
                lastMousePosition = Input.mousePosition;
                rotationVelocity = Vector2.zero;
            }
        }
        // Mouse / Touch Dragging
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            rotationVelocity = new Vector2(delta.x * rotationSpeed, delta.y * rotationSpeed);
            lastMousePosition = Input.mousePosition;
        }
        // Mouse / Touch Up
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// Smoothly rotates the globe so the selected marker faces the camera.
    /// </summary>
    public void FocusOnMarker(Transform markerTransform)
    {
        Vector3 directionToMarker = (markerTransform.position - transform.position).normalized;
        Vector3 targetForward = Camera.main != null ? -Camera.main.transform.forward : Vector3.forward;
        
        Quaternion rotOffset = Quaternion.FromToRotation(directionToMarker, targetForward);
        targetRotation = rotOffset * transform.rotation;
        isFocusing = true;
        rotationVelocity = Vector2.zero;
    }
}
