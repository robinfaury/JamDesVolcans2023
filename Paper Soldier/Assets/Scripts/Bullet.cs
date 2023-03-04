using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class Bullet : MonoBehaviour
{
    public static List<Bullet> bullets = new List<Bullet>();

    public float timealive = 10;
    private Renderer render;
    private float startTime;

    private Level mLevel;
    private Vector3Int mCell;

    void Start()
    {
        bullets.Add(this);
        render = GetComponent<Renderer>();

        if (Random.Range(0, 2) == 1)
        {
            render.material.color = Color.red;
            timealive = 2;
        }
        else
        {
            render.material.color = Color.blue;
            timealive = float.MaxValue;
        }
        startTime = Time.time; 
    }

    public void Init(Vector3Int cell, Level refLevel)
    {
        mLevel = refLevel;
        mCell = cell;
    }

    private void Update()
    {
        if (Time.time - startTime > timealive)
        {
            Destroy(gameObject);
            mLevel.map[mCell.x, mCell.y, mCell.z] = CellDatas.Empty;
        }
            
    }

}
