using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{

    public Level level;

    public enum Action
    {
        Forward,
        Right,
        Left,
        JumpForward,
        JumpRight,
        JumpLeft,
        Fall,
        AboutFace,
        DontMove
    }

    CellDatas[,,] Perceive(int width, int height, int depth)
    {
        CellDatas[,,] perseption = new CellDatas[width, height, depth];
        Vector3 position = transform.position;
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
    bool HasPath(CellDatas[,,] perseption)
    {
        if (perseption[1, 1, 1] == CellDatas.Empty)
        {
            return true;
        }
        return perseption[0, 1, 1] == CellDatas.Empty;
    }
    IEnumerator Start ()
    {
        transform.position = level.startPoint.transform.position;
        while (true)
        {
            Action action = Action.DontMove;
            int perseption_width = 5;
            int character_index_x = perseption_width/2;
            CellDatas[,,] perseption = Perceive(perseption_width, 2, 2);
            Vector3Int current_index_cell = level.PositionToIndex(transform.position);
            if (current_index_cell.y == 0)
            {
                action = Action.Fall;
            } else
            {
                if (level.map[current_index_cell.x, current_index_cell.y - 1, current_index_cell.z] == CellDatas.Empty)
                {
                    action = Action.Fall;
                }
            }
            if (action == Action.DontMove)
            {
                if (perseption[character_index_x, 1, 1] == CellDatas.Empty)
                {
                    action = perseption[character_index_x, 0, 1] == CellDatas.Empty ? Action.Forward : Action.JumpForward;
                }
                else if (perseption[character_index_x - 1, 1, 1] == CellDatas.Empty && perseption[character_index_x - 1, 1, 0] == CellDatas.Empty)
                {
                    action = perseption[character_index_x - 1, 0, 0] == CellDatas.Empty ? Action.Left : Action.JumpLeft;
                }
                else if (perseption[character_index_x + 1, 1, 1] == CellDatas.Empty && perseption[character_index_x + 1, 1, 0] == CellDatas.Empty)
                {
                    action = perseption[character_index_x + 1, 0, 0] == CellDatas.Empty ? Action.Right : Action.JumpRight;
                }
            }
            if (action == Action.DontMove)
            {
                action = Action.AboutFace;
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
                case Action.Fall:
                    transform.position -= transform.up;
                    break;
                case Action.AboutFace:
                    transform.forward = -transform.forward;
                    break;
                case Action.DontMove:
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (level == null || level.walkableMap == null) return;

        int x = level.ConvertCoordX (transform.position.x);
        int y = level.ConvertCoordY (transform.position.y);
        int z = level.ConvertCoordZ (transform.position.z);

        DrawAt(0, 0, 1);
        void DrawAt (int xo, int yo, int zo)
        {
            level.DrawCell(x + xo, y + yo, z + zo, level.walkableMap[x + xo, y + yo, z + zo] ? Color.red : Color.black);
        }
    }
}