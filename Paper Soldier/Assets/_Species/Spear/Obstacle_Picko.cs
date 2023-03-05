using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;

public class Obstacle_Picko : MonoBehaviour
{

    public Transform picko;
    public Transform inPosition;
    public Transform outPosition;
    public Transform kill;
    public Transform block;

    public int phaseDelta;
    [Range(0f, 1f)] public float tickPercentageIn = 0.5f;
    [Range(0f, 1f)] public float tickPercentageOut = 0.25f;

    int currentState;
    Vector3 velocity;
    void SetPosition()
    {
        if (currentState == 0)
        {
            picko.position = inPosition.position;
        }
        if (currentState == 2)
        {
            picko.position = outPosition.position;
        }
    }

    private void Awake()
    {
        currentState = phaseDelta;
        SetPosition();
        TickManager.onTick += () =>
        {
            currentState = ++currentState % 4;
            Vector3Int cell_front = g_currentLevel.PositionToIndex(block.position + block.forward);
            if (g_currentLevel.map[cell_front.x, cell_front.y, cell_front.z] != CellDatas.Solid)
            {
                if (currentState == 0)
                {
                    Vector3Int cell_base = g_currentLevel.PositionToIndex(kill.position);
                    Vector3Int cell_kill = g_currentLevel.PositionToIndex(kill.position - kill.forward);
                    g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] = CellDatas.Empty;
                    g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] = CellDatas.Empty;
                }
                SetPosition();
                if (currentState == 2)
                {
                    Vector3Int cell_base = g_currentLevel.PositionToIndex(kill.position);
                    Vector3Int cell_kill = g_currentLevel.PositionToIndex(kill.position - kill.forward);
                    if (g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] == CellDatas.Character || g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] == CellDatas.Character)
                    {
                        g_player.DeathByHit();
                    }
                    g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] = CellDatas.Solid;
                    g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] = CellDatas.Solid;
                }
            }
        };
    }

    void Update() {}
}