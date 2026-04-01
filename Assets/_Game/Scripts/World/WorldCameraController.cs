using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class WorldCameraController : MonoBehaviour
{
    private const float PanSpeed = 6f;
    private const float ZoomSpeed = 0.01f;
    private const float MinZoom = 2.5f;
    private const float MaxZoom = 14f;
    private static readonly Vector3 DefaultPosition = new Vector3(0f, 0f, -10f);

    private Camera _targetCamera;

    private void Awake()
    {
        CacheCamera();
        enabled = false;
    }

    public void SetWorldSimActive(bool isActive)
    {
        CacheCamera();
        if (_targetCamera == null)
        {
            enabled = false;
            return;
        }

        _targetCamera.orthographic = true;

        if (isActive)
        {
            _targetCamera.transform.position = DefaultPosition;
            _targetCamera.orthographicSize = Mathf.Clamp(_targetCamera.orthographicSize, MinZoom, MaxZoom);
        }

        enabled = isActive;
    }

    private void Update()
    {
        CacheCamera();
        if (_targetCamera == null || !_targetCamera.orthographic)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        Vector2 movement = Vector2.zero;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                movement.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                movement.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                movement.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                movement.y += 1f;
            }
        }

        if (movement.sqrMagnitude > 0f)
        {
            movement = movement.normalized;
            _targetCamera.transform.position += new Vector3(movement.x, movement.y, 0f) * PanSpeed * Time.deltaTime;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        float scrollY = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) <= 0.01f)
        {
            return;
        }

        float nextZoom = _targetCamera.orthographicSize - (scrollY * ZoomSpeed);
        _targetCamera.orthographicSize = Mathf.Clamp(nextZoom, MinZoom, MaxZoom);
    }

    private void CacheCamera()
    {
        if (_targetCamera == null)
        {
            _targetCamera = GetComponent<Camera>();
        }
    }
}
