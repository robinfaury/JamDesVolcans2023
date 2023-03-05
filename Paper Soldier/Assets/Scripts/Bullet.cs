using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
public class Bullet : MonoBehaviour
{
    public Transform CubeBulletPrefab;
    public Sound hit;

    public static List<Bullet> allBullets = new List<Bullet>();

    public float timealive = 10;
    private float startTime;

    private Level mLevel;
    private Vector3Int mCell;

    private bool isCube = false;

    private bool isDestructionBullet = false;

    public GameObject BulletSpawnEffectPrefab;
    private GameObject effect;
    private float effectSpawnTime;

    public void Init(Vector3Int cell, Level refLevel, bool isConstruction)
    {
        Renderer render = GetComponent<Renderer>();
        if (!isConstruction)
        {
            render.material.color = Color.red;
            timealive = 2;
            isDestructionBullet = true;
        }
        else
        {
            timealive = float.MaxValue;
            isDestructionBullet = false;
        }
        startTime = Time.time;
        mLevel = refLevel;
        mCell = cell;

        allBullets.Add(this);
    }

    public void OnBulletIsOnTarget()
    {
        hit.Play();
        effect = Instantiate(BulletSpawnEffectPrefab, transform.position, Quaternion.identity);
        effectSpawnTime = Time.time;
        if (isDestructionBullet)
        {
            List<Bullet> bulletToDestroy = new List<Bullet>();
            for (int i = 0; i < allBullets.Count; i++)
            {
                if (AreNeighbours(this, allBullets[i]))
                {
                    bulletToDestroy.Add(allBullets[i]);
                }
            }
            for (int i = 0; i < bulletToDestroy.Count; i++)
                bulletToDestroy[i].DestroyBullet();

            DestroyBullet();
        }
        else
        {
            bool sphereToCube = false;
            for (int i = 0; i < allBullets.Count; i++)
            {
                if (AreNeighbours(this, allBullets[i]))
                {
                    allBullets[i].SphereToCube();
                    sphereToCube = true;
                }
            }

            if (sphereToCube)
                SphereToCube();
        }
    }

    private bool AreNeighbours(Bullet a, Bullet b)
    {
        //Cas left/right
        if ( Mathf.Abs(a.mCell.x - b.mCell.x) == 1
            && a.mCell.y == b.mCell.y
            && a.mCell.z == b.mCell.z)
        {
            return true;
        }

        //Cas top/bottom
        if (a.mCell.x == b.mCell.x
            && Mathf.Abs(a.mCell.y - b.mCell.y) == 1
            && a.mCell.z == b.mCell.z)
        {
            return true;
        }

        //Cas front/behind
        if (a.mCell.x == b.mCell.x
            && a.mCell.y == b.mCell.y
            && Mathf.Abs( a.mCell.z-b.mCell.z) == 1)
        {
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (Time.time - startTime > timealive)
        {
            DestroyBullet();
        }
        if (effect != null
            && Time.time - effectSpawnTime > 3f)
            Destroy(effect);
    }

    private void DestroyBullet()
    {
        mLevel.map[mCell.x, mCell.y, mCell.z] = CellDatas.Empty;
        allBullets.Remove(this);
        Destroy(gameObject);
    }

    public static void DestroyAllBullets()
    {
        for (int i = 0; i < allBullets.Count; i++)
        {
            allBullets[i].mLevel.map[allBullets[i].mCell.x, allBullets[i].mCell.y, allBullets[i].mCell.z] = CellDatas.Empty;
            
            Destroy(allBullets[i].gameObject);
        }
        allBullets.Clear();
    }

    private void SphereToCube()
    {
        if (isCube) return;

        isCube = true;
        GetComponent<Renderer>().enabled = false;
        Transform cube =  Instantiate(CubeBulletPrefab, transform);
        cube.localScale = Vector3.one;
        cube.localPosition = Vector3.zero;
        List<int> availableRotation = new List<int>() { 0, 90, 180, 270 };
        cube.rotation = Quaternion.Euler(availableRotation[Random.Range(0, availableRotation.Count)], 
                                        0,
                                        availableRotation[Random.Range(0, availableRotation.Count)]);
        cube.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        StartCoroutine(GiggleAnim());
    }

    IEnumerator GiggleAnim()
    {
        Vector3 middleScale = Vector3.one * .9f;
        float lTime = 0;
        float lDuration = 0.3f;
        float lStartTime = Time.time;

        while(lTime < 1f)
        {
            lTime = (Time.time - lStartTime)/lDuration;
            transform.localScale = Mathfx.Sinerp(Vector3.one, middleScale, lTime);
            yield return null;
        }
        lStartTime = Time.time;
        while (lTime < 1f)
        {
            lTime = (Time.time - lStartTime) / lDuration;
            transform.localScale = Mathfx.Coserp(middleScale, Vector3.one, lTime);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        Destroy(effect);
    }


   
}
