using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;


public class ThrowBullet : MonoBehaviour
{
    public Level level;

    public Transform prefabBullet;
    public Transform StrawPivot;
    public Transform StrawOutput;

    private Transform bulletPreview; 

    new Camera camera;

    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one * 16;

    public float timeBetweenShot = 0;
    private float lastTimeShot = 0;

    private bool isThrowing = false;

    void Start()
    {
        camera = GetComponentInParent<Camera>();
        StrawPivot.localPosition = new Vector3(0, -0.41f, 0.457f);
        bulletPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        bulletPreview.GetComponent<Collider>().enabled = false;
    }

    void Update()
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.SphereCast(ray, .5f, out RaycastHit hitInfo))
        {
            //Set straw orientation
            StrawPivot.forward = StrawPivot.position - hitInfo.point;

            //Set preview and precompute voxelPos
            Vector3Int index = level.PositionToIndex(hitInfo.point);
            Vector3 cellCenter = level.GetCellCenter(index.x, index.y, index.z);
            if (level.map[index.x, index.y, index.z] == CellDatas.Empty)
            {
                bulletPreview.gameObject.SetActive(true);
                bulletPreview.position = cellCenter;
             
                //throw bullet
                if (Mouse.current.leftButton.wasPressedThisFrame
                && Time.time - lastTimeShot > timeBetweenShot
                && !isThrowing)
                {
                 
                    bulletPreview.gameObject.SetActive(false);
                    StartCoroutine(ThrowingBullet(cellCenter, index));
                }
            }
            else
            {
                bulletPreview.gameObject.SetActive(false);
            }
        }
    }
    
    IEnumerator ThrowingBullet(Vector3 targetPosition, Vector3Int cell)
    {
        isThrowing = true;
        lastTimeShot = Time.time;
        float startTime = Time.time;
        Vector3 startposition = StrawOutput.position;
        Quaternion rotationAcceleration = Quaternion.Euler(UnityEngine.Random.Range(-360f, 360f), 
                                                                UnityEngine.Random.Range(-360f, 360f),
                                                                UnityEngine.Random.Range(-360f, 360f));
        Transform bullet = Instantiate<Transform>(prefabBullet, StrawOutput.position, Quaternion.identity);

        
        bullet.gameObject.GetComponent<Bullet>().Init(cell, level);

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
        level.map[cell.x, cell.y, cell.z] = CellDatas.Solid;

        bullet.gameObject.GetComponent<Bullet>().CheckNeighbours();

        isThrowing = false;
    }
}
