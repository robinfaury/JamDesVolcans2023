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

    public Transform prefabBullet;
    public Transform StrawPivot;
    public Transform StrawOutput;

    private Transform bulletPreview;
    public Sound sarbacane;

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
        bulletPreview.transform.localScale = Vector3.one * .5f;
    }

    void Update()
    {
        if (g_isGamePlaying) UpdateThrowBullet();
    }

    Vector3 point;
    public void UpdateThrowBullet ()
    {
        if (g_currentLevel == null) return;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.SphereCast(ray, g_currentLevel.cellSize * 0.2f, out RaycastHit hitInfo)) {
            //Set straw orientation
            StrawPivot.forward = StrawPivot.position - hitInfo.point;
            point = hitInfo.point;
            //Set preview and precompute voxelPos
            Vector3Int index = g_currentLevel.PositionToIndex(hitInfo.point + Vector3.up * g_currentLevel.cellSize / 4);
            Vector3 cellCenter = g_currentLevel.GetCellCenter(index.x, index.y, index.z);
            if (g_currentLevel.map[index.x, index.y, index.z] == CellDatas.Empty) {
                bulletPreview.gameObject.SetActive(true);
                bulletPreview.position = Vector3.Lerp(bulletPreview.position, cellCenter, .4f);

                //throw bullet
                if ((Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                && Time.time - lastTimeShot > timeBetweenShot
                && !isThrowing) {
                    bulletPreview.gameObject.SetActive(false);
                    StartCoroutine(ThrowingBullet(cellCenter, index, Mouse.current.leftButton.wasPressedThisFrame));
                }
            }
            else {
                bulletPreview.gameObject.SetActive(false);
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
        g_currentLevel.map[cell.x, cell.y, cell.z] = CellDatas.Solid;

        bullet.gameObject.GetComponent<Bullet>().OnBulletIsOnTarget();

        isThrowing = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point, 0.3f);
    }
}
