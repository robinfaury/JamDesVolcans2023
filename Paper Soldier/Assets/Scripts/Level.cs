using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    // 0 = vide
    // 1 = collision
    // 2 = end level
    public CellDatas [,,] map;
    public bool [,,] walkableMap;

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

    // ========================================================================= GENERATION

    [Button("GenerateVoxel")]
    public void GenerateVoxel()
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

        map = new CellDatas [mapSizeX, mapSizeY, mapSizeZ];
        walkableMap = new bool[mapSizeX, mapSizeY, mapSizeZ];

        // Occlusion
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                for (int z = 0; z < mapSizeZ; z++) {
                    map[x, y, z] = IsCellOccluded(x, y, z) ? CellDatas.Solid : CellDatas.Empty;
                }
            }
        }

        // Walkable map
        for (int x = 0; x < mapSizeX; x++) {
            for (int z = 0; z < mapSizeZ; z++) {
                for (int y = mapSizeY - 1; y > 1; y--) {
                    bool topCell = map[x, y, z] == CellDatas.Empty;
                    bool bottomCell = map[x, y - 1, z] == CellDatas.Empty;
                    if (!bottomCell && topCell) {
                        walkableMap[x, y, z] = true;
                        break;
                    }
                }
            }
        }
    }

    int cacheX, cacheY, cacheZ;
    public bool CanWalkAt(Vector3 pos)
    {
        if (map == null) GenerateVoxel();
        cacheX = Mathf.FloorToInt((pos.x - minPointRounded.x) / cellSize);
        cacheY = Mathf.FloorToInt((pos.y - minPointRounded.y) / cellSize);
        cacheZ = Mathf.FloorToInt((pos.z - minPointRounded.z) / cellSize);
        if (cacheX < 0 || cacheX > mapSizeX - 1) return false;
        if (cacheY < 0 || cacheY > mapSizeY - 1) return false;
        if (cacheZ < 0 || cacheZ > mapSizeZ - 1) return false;
        return walkableMap[cacheX, cacheY, cacheZ];
    }

    // ========================================================================= TOOLS

    Vector3 GetCellCenter(int x, int y, int z)
    {
        return minPointRounded + new Vector3(
            (x + 0.5f) * cellSize * dirX,
            (y + 0.5f) * cellSize * dirY,
            (z + 0.5f) * cellSize * dirZ
        );
    }

    public bool IsCellOccluded(int x, int y, int z)
    {
        float exi = cellExigence / 2;
        return Physics.CheckBox(
            GetCellCenter(x, y, z),
            Vector3.one * cellSize * exi,
            Quaternion.identity,
            occlusionLayer
        );
    }

    // ========================================================================= GIZMOS

    private void OnDrawGizmos()
    {
        if (!showGizmos || map == null) return;
        if (autoRefresh) GenerateVoxel();

        // Map
        for (int x = 0; x < mapSizeX; x++) {
            for (int z = 0; z < mapSizeZ; z++) {
                for (int y = mapSizeY - 1; y > 1; y--) {
                    if (walkableMap[x, y, z]) DrawCell(x, y, z, walkableMapColor);
                }
            }
        }
    }

    // Draw a voxel cell
    public void DrawCell(int x, int y, int z, Color color)
    {
        Gizmos.color = color;
        Vector3 center = GetCellCenter(x, y, z) - Vector3.up * cellSize / 4;
        Gizmos.DrawCube(center, Vector3.one * 0.5f * cellSize);

#if UNITY_EDITOR
        Handles.color = new Color (color.r, color.g, color.b, 0.3f);
        Handles.DrawWireCube(center, Vector3.one);
#endif
    }
}

public enum CellDatas
{
    Empty,
    Solid,
    NextLevelCell
}