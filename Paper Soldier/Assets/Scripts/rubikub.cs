using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GameManager;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class rubikub : MonoBehaviour
{
    public Transform rubikub_transform;
    public int delay;
    public int duration_up;
    public int duration_down;

    int frame;
    int wait;

    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
        wait = delay;
        TickManager.onTick += () =>
        {
            if (--wait > 0)
            {
                return;
            }
            frame = ++frame % (duration_up + duration_down);
            if (frame == duration_down)
            {
                rubikub_transform.position -= new Vector3(0, 3, 0);
                Vector3Int[] cells = new Vector3Int[8];
                cells[0] = g_currentLevel.PositionToIndex(rubikub_transform.position + new Vector3(-1, -1, 1));
                cells[1] = g_currentLevel.PositionToIndex(rubikub_transform.position + new Vector3(-1, -1, 0));
                cells[2] = g_currentLevel.PositionToIndex(rubikub_transform.position + new Vector3(0, -1, 1));
                cells[3] = g_currentLevel.PositionToIndex(rubikub_transform.position + new Vector3(0, -1, 0));
                cells[4] = cells[0] + new Vector3Int(0, 1, 0);
                cells[5] = cells[1] + new Vector3Int(0, 1, 0);
                cells[6] = cells[2] + new Vector3Int(0, 1, 0);
                cells[7] = cells[3] + new Vector3Int(0, 1, 0);
                for (int i=0; i<8; i++)
                {
                    if (g_currentLevel.map[cells[i].x, cells[i].y, cells[i].z] == CellDatas.Character)
                    {
                        g_player.DeathByHit();
                    }
                    if (g_currentLevel.map[cells[i].x, cells[i].y, cells[i].z] == CellDatas.Boulette)
                    {
                        for (int j = 0; j < Bullet.allBullets.Count; j++)
                        {
                            if (Bullet.allBullets[j].mCell == cells[i])
                            {
                                Bullet.allBullets[j].DestroyBullet();
                                break;
                            }
                        }
                    }
                }
                
            }
            if (frame == duration_up)
            {
                rubikub_transform.position += new Vector3(0, 3, 0);

            }
            //currentState = ++currentState % 4;
            //Vector3Int cell_front = g_currentLevel.PositionToIndex(block.position + block.forward);
            //if (g_currentLevel.map[cell_front.x, cell_front.y, cell_front.z] != CellDatas.Boulette)
            //{
            //    if (currentState == 0)
            //    {
            //        Vector3Int cell_base = g_currentLevel.PositionToIndex(kill.position);
            //        Vector3Int cell_kill = g_currentLevel.PositionToIndex(kill.position - kill.forward);
            //        g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] = CellDatas.Empty;
            //        g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] = CellDatas.Empty;
            //    }
            //    SetPosition();
            //    if (currentState == 2)
            //    {
            //        Vector3Int cell_base = g_currentLevel.PositionToIndex(kill.position);
            //        Vector3Int cell_kill = g_currentLevel.PositionToIndex(kill.position - kill.forward);
            //        if (g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] == CellDatas.Character || g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] == CellDatas.Character)
            //        {
            //            g_player.DeathByHit();
            //        }
            //        g_currentLevel.map[cell_base.x, cell_base.y, cell_base.z] = CellDatas.Solid;
            //        g_currentLevel.map[cell_kill.x, cell_kill.y, cell_kill.z] = CellDatas.Solid;
            //    }
            //}
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
