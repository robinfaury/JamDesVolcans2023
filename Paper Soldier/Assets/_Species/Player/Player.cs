using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using static GameManager;

public class Player : MonoBehaviour
{

    [Title ("PARAMETERS")]
    [Range(0f, 1f)] public float movementTickPercent = 0.5f;
    public AnimationCurve walkFCurve;
    public AnimationCurve walkVCurve;
    public AnimationCurve jumpFCurve;
    public AnimationCurve jumpVCurve;
    public AnimationCurve rotationCurve;
    public Sound stepSound;
    public Sound jumpSound;
    public Sound fallSound;

    [Title ("REFERENCES")]
    public EventListener eventListener;
    public Animator animator;
    public Transform model;
    public GameObject footPrintPrefab;

    [Title ("DEBUG & RUNTIME")]
    public bool isMoving;

    float animationScale;

    IEnumerator Start ()
    {
        eventListener.onEvent += Event;
        while (true) {
            while (isMoving) {

                Action action = Action.DontMove;
                int perseption_width = 5;
                int character_index_x = perseption_width/2;
                CellDatas[,,] perseption = Perceive(perseption_width, 2, 2);
                Vector3Int current_index_cell = g_currentLevel.PositionToIndex(transform.position);

                if (current_index_cell.y == 0) {
                    action = Action.Fall;
                }
                else {
                    if (g_currentLevel.map[current_index_cell.x, current_index_cell.y - 1, current_index_cell.z] == CellDatas.Empty) {
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
                    else if (perseption[character_index_x - 1, 1, 1] == CellDatas.Solid && perseption[character_index_x - 1, 0, 0] == CellDatas.Solid)
                    {
                        action = Action.JumpLeft;
                    }
                    else if (perseption[character_index_x + 1, 1, 1] == CellDatas.Solid && perseption[character_index_x + 1, 0, 0] == CellDatas.Solid)
                    {
                        action = Action.JumpRight;
                    }
                }
                if (action == Action.DontMove) {
                    action = Action.AboutFace;
                }

                int tickAmount = 1;
                string trigger = "walk";
                if (action == Action.JumpForward || action == Action.JumpRight || action == Action.JumpLeft) {
                    tickAmount = 2;
                    trigger = "jump";
                }
                float tickDuration = TickManager.TickDuration * movementTickPercent * tickAmount;

                switch (action) {
                    case Action.Forward: 
                        Move(walkFCurve, walkVCurve, transform.position, transform.forward, tickDuration, trigger, action);
                        break;
                    case Action.Right: 
                        Move(walkFCurve, walkVCurve, transform.position, transform.right, tickDuration, trigger, action);
                        break;
                    case Action.Left: 
                        Move(walkFCurve, walkVCurve, transform.position, - transform.right, tickDuration, trigger, action);
                        break;
                    case Action.JumpForward: 
                        Move(jumpFCurve, jumpVCurve, transform.position, transform.up + transform.forward, tickDuration, trigger, action); 
                        break;
                    case Action.JumpRight: 
                        Move(jumpFCurve, jumpVCurve, transform.position, transform.up + transform.right, tickDuration, trigger, action); 
                        break;
                    case Action.JumpLeft: 
                        Move(jumpFCurve, jumpVCurve, transform.position, transform.up - transform.right, tickDuration, trigger, action); 
                        break;
                    case Action.Fall:
                        Fall();
                        break;
                    case Action.AboutFace:
                        transform.forward = -transform.forward;
                        break;
                    case Action.DontMove:
                        break;
                }

                yield return new WaitForSeconds(tickDuration);
            }
            yield return null;
        }
    }

    public void Update()
    {
        animator.Update(Time.deltaTime * animationScale);
    }

    public void Move(AnimationCurve f, AnimationCurve v, Vector3 start, Vector3 delta, float tickDuration, string trigger, Action action)
    {
        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            animator.SetTrigger(trigger);
            Vector3 end = start + delta;
            Vector3 directionStart = transform.forward;
            Vector3 directionEnd = (end - start).WithY (0).normalized;

            float duration = tickDuration * movementTickPercent;
            Vector3 df = (end - start).WithY(0).normalized;
            float dv = end.y - start.y;

            float percent = 0; while ((percent += Time.deltaTime / duration) < 1) {
                Vector3 position = start + df * f.Evaluate (percent) + Vector3.up * dv * v.Evaluate (percent);
                model.forward = Vector3.Lerp(directionStart, directionEnd, rotationCurve.Evaluate(percent));
                transform.position = position;
                yield return null;
            }
            transform.position = end;
            model.forward = directionEnd;
            g_onPlayerChanged?.Invoke(end);
        }
    }

    public void Fall ()
    {
        StartCoroutine(Routine()); IEnumerator Routine()
        {
            animator.SetTrigger("fall");
            Vector3 velocity = Vector3.zero;
            while (true) {
                velocity -= Vector3.up * Time.deltaTime;
                transform.position -= velocity * Time.deltaTime;
                yield return null;
            }
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
                    Vector3Int cell = g_currentLevel.PositionToIndex(position+(w-(int)(width/2))*transform.right+h*transform.up+d*transform.forward);
                    perseption[w, h, d] = g_currentLevel.map[cell.x, cell.y, cell.z];
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
        if (g_currentLevel == null || g_currentLevel.walkableMap == null) return;

        int x = g_currentLevel.ConvertCoordX (transform.position.x);
        int y = g_currentLevel.ConvertCoordY (transform.position.y);
        int z = g_currentLevel.ConvertCoordZ (transform.position.z);

        DrawAt(0, 0, 1);
        void DrawAt(int xo, int yo, int zo)
        {
            g_currentLevel.DrawCell(x + xo, y + yo, z + zo, g_currentLevel.walkableMap[x + xo, y + yo, z + zo] ? Color.red : Color.black);
        }
    }

    public void Event (string eventName)
    {
        if (eventName == "Step") {
            stepSound.Play();
            GameObject footPrint = Instantiate(footPrintPrefab);
            footPrint.transform.position = transform.position + Vector3.up * 0.05f;
            footPrint.transform.forward = transform.forward;
            footPrint.SetActive(true);
            Destroy(footPrint, 12);
        }
        if (eventName == "Jump") jumpSound.Play();
        if (eventName == "FallSound") fallSound.Play();
        if (eventName == "FreezePoint") animationScale = 0;
    }

    public void Death ()
    {
        g_OnPlayerDeath?.Invoke();
    }
}