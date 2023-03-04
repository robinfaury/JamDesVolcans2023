using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;


public class ThrowBullet : MonoBehaviour
{
    public Transform prefabBullet;
    public Transform StrawPivot;
    public Transform StrawOutput;

    new Camera camera;

    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one * 16;

    void Start()
    {
        camera = GetComponentInParent<Camera>();
        StrawPivot.localPosition = new Vector3(0, -0.41f, 0.457f);
    }

    void Update()
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        //TODO : interact with voxel
        //tmp with Physics
        if(Physics.Raycast(ray,out RaycastHit hitInfo))
        {
            StrawPivot.forward = StrawPivot.position - hitInfo.point;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(ThrowingBullet(hitInfo.point));
        }
    }


    IEnumerator ThrowingBullet(Vector3 targetPosition)
    {
        float startTime = Time.time;
        Vector3 startposition = StrawOutput.position;
        Quaternion rotationAcceleration = Quaternion.Euler(Random.Range(-360f, 360f), Random.Range(-360f, 360f), Random.Range(-360f, 360f));
        Transform bullet = Instantiate<Transform>(prefabBullet, StrawOutput.position, Quaternion.identity);

        float lDuration = .5f;
        float lTime = 0f;

        while (lTime < lDuration)
        {
            bullet.position = Vector3.Lerp(startposition, targetPosition,lTime*lTime);
            bullet.localScale = Vector3.Lerp(startScale, endScale, lTime*lTime);
            bullet.localRotation *= rotationAcceleration;
            lTime = (Time.time - startTime)/lDuration;
            yield return null;
        }

        bullet.position = targetPosition;
        bullet.localScale = endScale; 

    }
}
