using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (Camera))]
public class GameCamera : MonoBehaviour
{

    public Transform throwModel;
    public Transform bullet;

    Plane plane;
    new Camera camera;

    public void Start()
    {
        plane = new Plane (Vector3.up, Vector3.zero);
        camera = GetComponent<Camera>();
    }

    public void Update ()
    {
        Ray ray = camera.ScreenPointToRay (Mouse.current.position.ReadValue());
        plane.Raycast(ray, out float dist);
        Vector3 groundPos = ray.GetPoint (dist);
        Vector3 dir = (groundPos - transform.position).normalized;
        throwModel.transform.forward = dir;

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Transform inst = Instantiate<Transform>(bullet);
            inst.position = groundPos;
        }
    }
}