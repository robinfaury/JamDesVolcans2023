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
        if (level == null || level.walkableMap == null) return;

        int x = level.ConvertCoordX (transform.position.x);
        int y = level.ConvertCoordY (transform.position.y);
        int z = level.ConvertCoordZ (transform.position.z);

        Debug.Log(x+ " " + y + " " + z);

        DrawAt(0, 0, 1);
        void DrawAt (int xo, int yo, int zo)
        {
            level.DrawCell(x + xo, y + yo, z + zo, level.walkableMap[x + xo, y + yo, z + zo] ? Color.red : Color.black);
        }
    }
}