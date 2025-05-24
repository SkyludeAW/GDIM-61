using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private RectTransform CameraBoundsTransform;
    private Vector3[] CameraBoundsCorners = new Vector3[4];
    private Rect CameraBounds;

    private Vector3 _movement;
    public float CameraSpeed;

    private Vector2 _scroll;
    public float CameraMinSize = 1f, CameraMaxSize = 8f, CameraSizeChangeValue = 0.5f, CameraSpeedUpMultiplier = 4f;

    private void Start() {
        if (CameraBoundsTransform != null) {
            CameraBoundsTransform.GetWorldCorners(CameraBoundsCorners);
            CameraBounds = new Rect(
                CameraBoundsCorners[0].x,
                CameraBoundsCorners[0].y,
                CameraBoundsCorners[2].x - CameraBoundsCorners[0].x,
                CameraBoundsCorners[1].y - CameraBoundsCorners[0].y 
            );
        }
    }

    private void Update() {
        float speedUp = 1f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift))
            speedUp *= CameraSpeedUpMultiplier;

        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");
        _movement.Normalize();

        _scroll = Input.mouseScrollDelta * CameraSizeChangeValue;
        
        if (CameraLocator.Instance != null && CameraLocator.Instance.PlayerCinemachineCamera != null) {
            float newCameraSize = CameraLocator.Instance.PlayerCinemachineCamera.Lens.OrthographicSize - _scroll.y;
            CameraLocator.Instance.PlayerCinemachineCamera.Lens.OrthographicSize = Mathf.Clamp(newCameraSize, CameraMinSize, CameraMaxSize);
        }

        transform.position += CameraSpeed * speedUp * Time.deltaTime * _movement;

        if (CameraBoundsTransform != null)
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, CameraBounds.xMin, CameraBounds.xMax), Mathf.Clamp(transform.position.y, CameraBounds.yMin, CameraBounds.yMax));
    }
}
