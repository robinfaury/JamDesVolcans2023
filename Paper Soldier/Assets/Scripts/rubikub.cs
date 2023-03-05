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
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
