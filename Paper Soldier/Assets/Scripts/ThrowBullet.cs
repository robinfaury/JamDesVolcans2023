using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class ThrowBullet : MonoBehaviour
{

    public Transform cursorPreview;
    public Transform prefabBullet;
    public Transform StrawPivot;
    public Transform StrawOutput;

    //private Transform bulletPreview;

    public Sound sarbacane;

    new Camera camera;

    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one * 16;

    public float timeBetweenShot = 0;
    private float lastTimeShot = 0;

    private bool isThrowing = false;
    private bool canThrow;

    void Start()
    {
        camera = GetComponentInParent<Camera>();
        StrawPivot.localPosition = new Vector3(0, -0.41f, 0.457f);

        /*
        bulletPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        bulletPreview.GetComponent<Collider>().enabled = false;
        bulletPreview.transform.localScale = Vector3.one * .5f;
        */
    }

    void Update()
    {
        if (g_isGamePlaying && canThrow) UpdateThrowBullet();
    }

    Vector3 point, normal;
    public void UpdateThrowBullet ()
    {
        if (g_currentLevel == null) return;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.SphereCast(ray, 0.1f, out RaycastHit hitInfo)) {

            //Set straw orientation
            StrawPivot.forward = StrawPivot.position - hitInfo.point;
            point = hitInfo.point;
            normal = hitInfo.normal;

            //Set preview and precompute voxelPos
            Vector3Int index = g_currentLevel.PositionToIndex(hitInfo.point + hitInfo.normal * g_currentLevel.cellSize / 4);
            Vector3 cellCenter = g_currentLevel.GetCellCenter(index.x, index.y, index.z);

            if (g_currentLevel.map[index.x, index.y, index.z] == CellDatas.Empty) {
                cursorPreview.gameObject.SetActive(true);
                cursorPreview.position = Vector3.Lerp(cursorPreview.position, cellCenter, .4f);
                cursorPreview.transform.up = normal;

                //throw bullet
                if ((Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                && Time.time - lastTimeShot > timeBetweenShot
                && !isThrowing) {
                    cursorPreview.gameObject.SetActive(false);
                    StartCoroutine(ThrowingBullet(cellCenter, index, Mouse.current.leftButton.wasPressedThisFrame));
                }
            }
            else {
                cursorPreview.gameObject.SetActive(false);
            }
        }
    }
    
    IEnumerator ThrowingBullet(Vector3 targetPosition, Vector3Int cell,bool isConstruction)
    {
        isThrowing = true;
        sarbacane.Play();
        lastTimeShot = Time.time;
        float startTime = Time.time;
        Vector3 startposition = StrawOutput.position;
        Quaternion rotationAcceleration = Quaternion.Euler(UnityEngine.Random.Range(-360f, 360f), 
                                                                UnityEngine.Random.Range(-360f, 360f),
                                                                UnityEngine.Random.Range(-360f, 360f));
        Transform bullet = Instantiate<Transform>(prefabBullet, StrawOutput.position, Quaternion.identity);

        
        bullet.gameObject.GetComponent<Bullet>().Init(cell, g_currentLevel, isConstruction);

        float lDuration = .5f;
        float lTime = 0f;

        while (lTime < 1f)
        {
            bullet.position = Vector3.Lerp(startposition, targetPosition,lTime*lTime);
            bullet.localScale = Vector3.Lerp(startScale, endScale, lTime*lTime);
            bullet.localRotation *= rotationAcceleration;
            lTime = (Time.time - startTime)/lDuration;
            yield return null;
        }

        bullet.position = targetPosition;
        bullet.localScale = endScale;
        g_currentLevel.map[cell.x, cell.y, cell.z] = CellDatas.Boulette;

        bullet.gameObject.GetComponent<Bullet>().OnBulletIsOnTarget();

        isThrowing = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point, 0.2f);
        Gizmos.DrawLine(point, point + normal);
    }

    public void AllowThrow ()
    {
        canThrow = true;
    }

    public void DisallowThrow ()
    {
        canThrow = false;
    }
}