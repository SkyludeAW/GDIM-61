using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Vector3 _movement;
    public float CameraSpeed;

    private void Update() {
        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");
        _movement.Normalize();

        transform.position += _movement * CameraSpeed * Time.deltaTime;
    }
}
