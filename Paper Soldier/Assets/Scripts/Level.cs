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
    public Transform startPoint;
    public Transform endPoint;
    public Vector3 offset;
    [Min(0.5f)] public float cellSize = 2;
    [Range(0.1f, 1f)] public float cellExigence = 0.5f;
    public LayerMask occlusionLayer;

    [Title ("DEBUGGIN & RUNTIME")]
    public bool showGizmos;
    public bool autoRefresh;
    public Color walkableMapColor = Color.red;
    public CellDatas [,,] map;
    public bool [,,] walkableMap;
    public Vector3 maxPointRounded;
    public Vector3 minPointRounded;
    public int dirX;
    public int dirY;
    public int dirZ;
    public float sizeX;
    public float sizeY;
    public float sizeZ;
    public int mapSizeX;
    public int mapSizeY;
    public int mapSizeZ;
    public Vector3 cellWidth;
    public Vector3 cellLength;
    public Vector3 cellHeight;

    // ========================================================================= GENERATION

    [Button("GenerateVoxel")]
    public void GenerateVoxel()
    {
        maxPointRounded = new Vector3(
            Mathf.FloorToInt(maxPoint.position.x),
            Mathf.FloorToInt(maxPoint.position.y),
            Mathf.FloorToInt(maxPoint.position.z)
        ) + offset;
        minPointRounded = new Vector3(
            Mathf.FloorToInt(minPoint.position.x),
            Mathf.FloorToInt(minPoint.position.y),
            Mathf.FloorToInt(minPoint.position.z)
        ) + offset;

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

    // ========================================================================= TOOLS

    public Vector3 GetCellCenter(int x, int y, int z)
    {
        return minPointRounded + new Vector3(
            (x + 0.5f) * cellSize * dirX,
            (y + 0.5f) * cellSize * dirY,
            (z + 0.5f) * cellSize * dirZ
        );
    }

    public Vector3 GetCellBottomAt(Vector3 position)
    {
        Vector3 index = PositionToIndex (position);
        return minPointRounded + new Vector3(
            (index.x + 0.5f) * cellSize * dirX,
            (index.y) * cellSize * dirY,
            (index.z + 0.5f) * cellSize * dirZ
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

        Vector3Int indexStart = PositionToIndex (startPoint.position);
        Vector3Int indexEnd = PositionToIndex (endPoint.position);
        DrawCell(indexStart.x, indexStart.y, indexStart.z, Color.green);
        DrawCell(indexEnd.x, indexEnd.y, indexEnd.z, Color.green);
    }

    // Draw a voxel cell
    public void DrawCell(int x, int y, int z, Color color)
    {
        Gizmos.color = color;
        Vector3 center = GetCellCenter(x, y, z);
        Gizmos.DrawCube(center, Vector3.one * 0.5f * cellSize);
        Gizmos.DrawSphere(GetCellBottomAt(GetCellCenter(x, y, z)), 0.1f);

#if UNITY_EDITOR
        Handles.color = new Color (color.r, color.g, color.b, 0.05f);
        Handles.DrawWireCube(center, Vector3.one);
#endif
    }

    public int ConvertCoordX (float x)
    {
        float dist = Mathf.Abs (x - minPointRounded.x);
        return Mathf.Clamp (Mathf.FloorToInt(dist / cellSize), 0, mapSizeX - 1);
    }

    public int ConvertCoordY(float y)
    {
        float dist = Mathf.Abs (y - minPointRounded.y);
        return Mathf.Clamp(Mathf.FloorToInt(dist / cellSize), 0, mapSizeY - 1);
    }

    public int ConvertCoordZ(float z)
    {
        float dist = Mathf.Abs (z - minPointRounded.z);
        return Mathf.Clamp(Mathf.FloorToInt(dist / cellSize), 0, mapSizeZ - 1);
    }

    public Vector3Int PositionToIndex(Vector3 position)
    {
        return new Vector3Int(
            ConvertCoordX(position.x),
            ConvertCoordY(position.y),
            ConvertCoordZ(position.z));
    }
}

public enum CellDatas
{
    Empty,
    Solid,
    NextLevelCell
}