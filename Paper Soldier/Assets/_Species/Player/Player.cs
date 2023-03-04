using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;

public class Player : MonoBehaviour
{

    [Title ("PARAMETERS")]
    [Range(0f, 1f)] public float movementTickPercent = 0.5f;

    [Title ("REFERENCES")]
    public Animator animator;

    [Title ("DEBUG & RUNTIME")]
    public Level level;
    public bool isMoving;

    float animationScale;

    IEnumerator Start ()
    {
        transform.position = level.startPoint.transform.position;
        while (true) {
            while (isMoving) {

                Action action = Action.DontMove;
                int perseption_width = 5;
                int character_index_x = perseption_width/2;
                CellDatas[,,] perseption = Perceive(perseption_width, 2, 2);
                Vector3Int current_index_cell = level.PositionToIndex(transform.position);

                if (current_index_cell.y == 0) {
                    action = Action.Fall;
                }
                else {
                    if (level.map[current_index_cell.x, current_index_cell.y - 1, current_index_cell.z] == CellDatas.Empty) {
                        action = Action.Fall;
                    }
                }
                if (action == Action.DontMove) {
                    if (perseption[character_index_x, 1, 1] == CellDatas.Empty) {
                        action = perseption[character_index_x, 0, 1] == CellDatas.Empty ? Action.Forward : Action.JumpForward;
                    }
                    else if (perseption[character_index_x - 1, 1, 1] == CellDatas.Empty && perseption[character_index_x - 1, 1, 0] == CellDatas.Empty) {
                        action = perseption[character_index_x - 1, 0, 0] == CellDatas.Empty ? Action.Left : Action.JumpLeft;
                    }
                    else if (perseption[character_index_x + 1, 1, 1] == CellDatas.Empty && perseption[character_index_x + 1, 1, 0] == CellDatas.Empty) {
                        action = perseption[character_index_x + 1, 0, 0] == CellDatas.Empty ? Action.Right : Action.JumpRight;
                    }
                }
                if (action == Action.DontMove) {
                    action = Action.AboutFace;
                }

                switch (action) {
                    case Action.Forward: Move(transform.position, transform.forward, action); break;
                    case Action.Right: Move(transform.position, transform.right, action); break;
                    case Action.Left: Move(transform.position, - transform.right, action); break;
                    case Action.JumpForward: Move(transform.position, transform.up + transform.forward, action); break;
                    case Action.JumpRight: Move(transform.position, transform.up + transform.right, action); break;
                    case Action.JumpLeft: Move(transform.position, transform.up - transform.right, action); break;
                    case Action.Fall: Move(transform.position, - transform.up, action); break;
                    case Action.AboutFace:
                        transform.forward = -transform.forward;
                        break;
                    case Action.DontMove:
                        break;
                }
                yield return new WaitForSeconds(TickManager.TickDuration);
            }

            yield return null;
        }
    }

    public void Update()
    {
        animator.Update(Time.deltaTime * animationScale);
    }

    public void Move (Vector3 start, Vector3 delta, Action action)
    {
        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            string trigger = "walk";
            if (action == Action.JumpForward) trigger = "jump";
            animator.SetTrigger(trigger);

            Vector3 end = start + delta;
            float duration = TickManager.TickDuration * movementTickPercent;
            //animationScale = 1f / duration;

            float percent = 0; while ((percent += Time.deltaTime / duration) < 1) {
                Vector3 position = Vector3.Lerp (start, end, percent);
                transform.position = position;
                yield return null;
            }
            transform.position = end;
        }
    }

    public void Rotate ()
    {

    }

    public void StartMovement ()
    {
        isMoving = true;
    }

    public void StopMovement ()
    {
        isMoving = false;
    }

    CellDatas[,,] Perceive(int width, int height, int depth)
    {
        CellDatas[,,] perseption = new CellDatas[width, height, depth];
        Vector3 position = transform.position;
        for (int w = 0; w < width; ++w) {
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
        if (perseption[1, 1, 1] == CellDatas.Empty) {
            return true;
        }
        return perseption[0, 1, 1] == CellDatas.Empty;
    }

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

    private void OnDrawGizmos()
    {
        if (level == null || level.walkableMap == null) return;

        int x = level.ConvertCoordX (transform.position.x);
        int y = level.ConvertCoordY (transform.position.y);
        int z = level.ConvertCoordZ (transform.position.z);

        DrawAt(0, 0, 1);
        void DrawAt(int xo, int yo, int zo)
        {
            level.DrawCell(x + xo, y + yo, z + zo, level.walkableMap[x + xo, y + yo, z + zo] ? Color.red : Color.black);
        }
    }
}