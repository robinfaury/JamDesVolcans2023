using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class pins : MonoBehaviour
{
    public Transform pin_transform;

    // Start is called before the first frame update
    void Start()
    {
        TickManager.onTick += () =>
        {
            Vector3Int cell = g_currentLevel.PositionToIndex(pin_transform.position);
            if (g_currentLevel.map[cell.x, cell.y, cell.z] == CellDatas.Character)
            {
                g_player.DeathByHit();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
