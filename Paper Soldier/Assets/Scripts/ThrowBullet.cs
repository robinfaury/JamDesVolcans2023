using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static GameManager;

public class ThrowBullet : MonoBehaviour
{
    public Transform cursorPreview;
    public Transform prefabBullet;
    public Transform StrawPivot;
    public Transform StrawOutput;
    //UI
    private Transform UICanvas;
    public Sprite bulletSprite;
    private RectTransform UIGroup;
    private List<Image> bulletSprites = new List<Image>();


    public Sound sarbacane;

    new Camera camera;

    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one * 16;

    public float timeBetweenShot = 0;
    private float lastTimeShot = 0;

    private int bulletCount = 5;

    private bool isThrowing = false;

    void Start()
    {
        UICanvas = GameObject.FindObjectOfType<Canvas>().transform;
        camera = GetComponentInParent<Camera>();
        StrawPivot.localPosition = new Vector3(0, -0.41f, 0.457f);
        CreateUI();
        /*
        bulletPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        bulletPreview.GetComponent<Collider>().enabled = false;
        bulletPreview.transform.localScale = Vector3.one * .5f;
        */
    }

    void CreateUI()
    {
        GameObject bulletLayout = new GameObject("BulletLayout");
        HorizontalLayoutGroup hlg = bulletLayout.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        bulletLayout.transform.SetParent(UICanvas);
        UIGroup = (RectTransform)bulletLayout.transform;
        UIGroup.SetAsFirstSibling();
        UIGroup.anchorMin = new Vector2(0, 1);
        UIGroup.anchorMax = new Vector2(1, 1);
        UIGroup.pivot = new Vector2(0.5f, 1);
        UIGroup.offsetMin = Vector2.zero;
        UIGroup.offsetMax = Vector2.zero;
        UIGroup.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 120);
        
    }


    void InitUI(int bulletCount)
    {
        for(int i = 0; i < UIGroup.childCount; i++)
        {
            Destroy(UIGroup.GetChild(i).gameObject);
        }
        bulletSprites.Clear();

        for(int i =0; i < bulletCount; i++)
        {
            Image lImg = new GameObject("bulletImg").AddComponent<Image>();
            lImg.rectTransform.SetParent(UIGroup);
            lImg.sprite = bulletSprite;
            lImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50);
            lImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
            bulletSprites.Add(lImg);
        }
    }

    void UpdateUI()
    {
        for(int i =0; i < bulletSprites.Count;i++)
        {
            bulletSprites[i].enabled = i < bulletCount;
        }

    }


    void Update()
    {
        if (g_isGamePlaying) UpdateThrowBullet();
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
                cursorPreview.gameObject.SetActive(bulletCount > 0);
                cursorPreview.position = Vector3.Lerp(cursorPreview.position, cellCenter, .4f);
                cursorPreview.transform.up = normal;

                //throw bullet
                if ((Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                && Time.time - lastTimeShot > timeBetweenShot
                && !isThrowing) {
                    cursorPreview.gameObject.SetActive(false);
                    if (bulletCount > 0)
                        StartCoroutine(ThrowingBullet(cellCenter, index, Mouse.current.leftButton.wasPressedThisFrame));
                    else
                        Debug.Log("no more bullet");
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
        bulletCount--;
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
            if (bullet == null)
                yield break;

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

        UpdateUI();

        isThrowing = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point, 0.2f);
        Gizmos.DrawLine(point, point + normal);
    }

    public void Reset()
    {
        Bullet.DestroyAllBullets();
        bulletCount = g_currentLevel.maxBullet;
        InitUI(bulletCount);
    }
}
