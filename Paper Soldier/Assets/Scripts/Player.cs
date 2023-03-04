using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Level level;

    Vector3 directionForward;
    Vector3 directionLeft;

    IEnumerator Start ()
    {
        directionForward = transform.forward;
        directionLeft = -transform.right;

        while (true) {
            if (GetAt(1, 0, 0, out Vector3 a) && GetAt(1, 0, 1, out Vector3 b)) {
                transform.position = a;
            }
            else if (GetAt(0, 0, 1, out Vector3 c)) {
                transform.position = c;
            }
            else if (GetAt(-1, 0, 0, out Vector3 d)) {
                transform.position = d;
            }
            yield return new WaitForSeconds(0.5f);
        }

        bool GetAt (int x, int y, int z, out Vector3 pos)
        {
            Vector3 testPos = transform.position + directionForward * z + directionLeft * x + Vector3.up * y;
            pos = testPos;
            return level.CanWalkAt(testPos);
        }
    }

    private void OnDrawGizmos()
    {
        //if (level.walkMap == null) return;
        int x = Mathf.Clamp(Mathf.FloorToInt((transform.position.x - level.minPoint.position.x) / level.cellSize), 0, level.mapSizeX - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((transform.position.y - level.minPoint.position.y) / level.cellSize), 0, level.mapSizeY - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt((transform.position.z - level.minPoint.position.z) / level.cellSize), 0, level.mapSizeZ - 1);

        DrawAt(0, 0, 1);
        void DrawAt (int xo, int yo, int zo)
        {
            level.DrawCell(x + xo, y + yo, z + zo, level.walkableMap[x + xo, y + yo, z + zo] ? Color.red : Color.black);
        }
    }
}