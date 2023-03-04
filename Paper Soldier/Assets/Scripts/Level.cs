using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Level : MonoBehaviour
{
    [Title ("PARAMETRES")]
    public Transform minPoint;
    public Transform maxPoint;
    [Min(0.5f)]public float cellSize = 2;
    [Range(0.1f, 1f)]public float cellExigence = 0.5f;
    public LayerMask occlusionLayer;

    [Title ("DEBUG")]
    public bool showGizmos;
    public bool autoRefresh;
    public Color walkableMapColor = Color.red;

    public bool[,,] map;
    public bool[,,] walkMap;

    Vector3 maxPointRounded;
    Vector3 minPointRounded;
    float dirX;
    float dirY;
    float dirZ;
    float sizeX;
    float sizeY;
    float sizeZ;
    public int mapSizeX;
    public int mapSizeY;
    public int mapSizeZ;
    Vector3 cellWidth;
    Vector3 cellLength;
    Vector3 cellHeight;

    [Button("BAKE")]
    public void Bake()
    {
        maxPointRounded = new Vector3(Mathf.CeilToInt(maxPoint.position.x), Mathf.CeilToInt(maxPoint.position.y), Mathf.CeilToInt(maxPoint.position.z));
        minPointRounded = new Vector3(Mathf.CeilToInt(minPoint.position.x), Mathf.CeilToInt(minPoint.position.y), Mathf.CeilToInt(minPoint.position.z));

        sizeX = Mathf.Abs (maxPointRounded.x - minPointRounded.x);
        sizeY = Mathf.Abs (maxPointRounded.y - minPointRounded.y);
        sizeZ = Mathf.Abs (maxPointRounded.z - minPointRounded.z);

        dirX = (maxPointRounded.x - minPointRounded.x) > 0 ? 1 : -1;
        dirY = (maxPointRounded.y - minPointRounded.y) > 0 ? 1 : -1;
        dirZ = (maxPointRounded.z - minPointRounded.z) > 0 ? 1 : -1;

        mapSizeX = Mathf.CeilToInt(sizeX / cellSize);
        mapSizeY = Mathf.CeilToInt(sizeY / cellSize);
        mapSizeZ = Mathf.CeilToInt(sizeZ / cellSize);

        cellWidth = Vector3.right * cellSize;
        cellLength = Vector3.forward * cellSize;
        cellHeight = Vector3.up * cellSize;

        map = new bool [mapSizeX, mapSizeY, mapSizeZ];
        walkMap = new bool [mapSizeX, mapSizeY, mapSizeZ];

        // Occlusion
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                for (int z = 0; z < mapSizeZ; z++) {
                    map[x, y, z] = !OcclusionOn(x, y, z);
                }
            }
        }

        // Walkable map
        for (int x = 0; x < mapSizeX; x++) {
            for (int z = 0; z < mapSizeZ; z++) {
                for (int y = mapSizeY - 1; y > 1; y--) {
                    bool topCell = map[x, y, z];
                    bool bottomCell = map[x, y - 1, z];
                    if (!bottomCell && topCell) {
                        walkMap[x, y, z] = true;
                        break;
                    }
                }
            }
        }
    }

    static int cacheX;
    static int cacheY;
    static int cacheZ;
    public bool CanWalkAt(Vector3 pos)
    {
        if (map == null) Bake();
        cacheX = Mathf.FloorToInt((pos.x - minPointRounded.x) / cellSize);
        cacheY = Mathf.FloorToInt((pos.y - minPointRounded.y) / cellSize);
        cacheZ = Mathf.FloorToInt((pos.z - minPointRounded.z) / cellSize);
        if (cacheX < 0 || cacheX > mapSizeX - 1) return false;
        if (cacheY < 0 || cacheY > mapSizeY - 1) return false;
        if (cacheZ < 0 || cacheZ > mapSizeZ - 1) return false;
        return walkMap[cacheX, cacheY, cacheZ];
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || walkMap == null) return;
        if (autoRefresh) Bake();

        // Map
        for (int x = 0; x < mapSizeX; x++) {
            for (int z = 0; z < mapSizeZ; z++) {
                for (int y = mapSizeY - 1; y > 1; y--) {
                    if (walkMap[x, y, z]) DrawCell(x, y, z, walkableMapColor);
                }
            }
        }
    }

    // Cell
    public void DrawCell(int x, int y, int z, Color color)
    {
        Gizmos.color = color;
        Vector3 center = GetCellCenter(x, y, z);
        Vector3 scale = new Vector3 (
                0.8f,
                0.3f,
                0.8f
            ) * cellSize;
        Gizmos.DrawCube(center, scale);
    }

    Vector3 GetCellCenter(int x, int y, int z)
    {
        return minPointRounded + new Vector3(
            (x + 0.5f) * cellSize * dirX,
            (y + 0.5f) * cellSize * dirY,
            (z + 0.5f) * cellSize * dirZ
        );
    }

    public bool OcclusionOn(int x, int y, int z)
    {
        float exi = cellExigence / 2;
        return Physics.CheckBox(
            GetCellCenter(x, y, z),
            Vector3.one * cellSize * exi,
            Quaternion.identity,
            occlusionLayer
        );
    }
}