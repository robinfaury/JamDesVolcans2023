using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Level level;

    Vector3 directionForward;
    Vector3 directionLeft;

    public enum Action
    {
        Forward,
        Right,
        Left,
        JumpForward,
        JumpRight,
        JumpLeft,
        DontMove
    }

    CellDatas[,,] Perceive(int width, int height, int depth)
    {
        CellDatas[,,] perseption = new CellDatas[width, height, depth];
        Vector3 position = transform.position + directionForward; // Position in front of the character
        for (int w=0; w<width; ++w) {
            for (int h = 0; h < height; ++h) {
                for (int d = 0; d < depth; ++d) {
                    Vector3Int cell = level.PositionToIndex(position+(w-(int)(width/2))*transform.right+h*transform.up+d*transform.forward);
                    perseption[w, h, d] = level.map[cell.x, cell.y, cell.z];
                }
            }
        }
        return perseption;
    }
    IEnumerator Start ()
    {
        directionForward = transform.forward;
        directionLeft = -transform.right;

        while (true) {
            Vector3Int indices = level.PositionToIndex(transform.position);
            level.map[indices.x, indices.y, indices.z]++;
            CellDatas[,,] perseption = Perceive(3, 2, 1);
            Action action = Action.DontMove;

            if (perseption[1, 0, 0] == CellDatas.Empty && perseption[1, 1, 0] == CellDatas.Empty)
            {
                action = Action.Forward;
            } else if (perseption[1,1,0]==CellDatas.Solid && perseption[0, 1, 0] == CellDatas.Solid && perseption[2, 1, 0] == CellDatas.Empty)
            {
                action = Action.Right;
            } else if (perseption[1, 1, 0] == CellDatas.Solid && perseption[0, 1, 0] == CellDatas.Empty && perseption[2, 1, 0] == CellDatas.Solid)
            {
                action = Action.Left;
            } else if (perseption[1, 0, 0] == CellDatas.Solid && perseption[1, 1, 0] == CellDatas.Empty)
            {
                action = Action.JumpForward;
            }

            switch (action)
            {
                case Action.Forward:
                    transform.position += transform.forward;
                    break;
                case Action.Right:
                    transform.position += transform.right;
                    break;
                case Action.Left:
                    transform.position -= transform.right;
                    break;
                case Action.JumpForward:
                    transform.position += transform.up;
                    transform.position += transform.forward;
                    break;
                case Action.JumpRight:
                    transform.position += transform.up;
                    transform.position += transform.right;
                    break;
                case Action.JumpLeft:
                    transform.position += transform.up;
                    transform.position -= transform.right;
                    break;
                case Action.DontMove:
                    break;
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