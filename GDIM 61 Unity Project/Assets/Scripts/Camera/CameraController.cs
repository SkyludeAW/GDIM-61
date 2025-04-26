using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Vector3 _movement;
    public float CameraSpeed;

    private Vector2 _scroll;
    public float CameraMinSize = 1f, CameraMaxSize = 8f, CameraSizeChangeValue = 0.5f;

    private void Update() {
        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");
        _movement.Normalize();

        _scroll = Input.mouseScrollDelta * CameraSizeChangeValue;
        
        if (CameraLocator.Instance != null && CameraLocator.Instance.PlayerCinemachineCamera != null) {
            float newCameraSize = CameraLocator.Instance.PlayerCinemachineCamera.Lens.OrthographicSize - _scroll.y;
            CameraLocator.Instance.PlayerCinemachineCamera.Lens.OrthographicSize = Mathf.Clamp(newCameraSize, CameraMinSize, CameraMaxSize);
        }

        transform.position += _movement * CameraSpeed * Time.deltaTime;
    }
}
