using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using static GameManager;
using UnityEngine.TextCore.Text;
using System.Linq;

public class Player : MonoBehaviour
{

    [Title ("PARAMETERS")]
    [Range(0f, 1f)] public float movementTickPercent = 0.5f;
    public float deathExplosionDuration = 10;
    public float rebootAfter = 4;
    public AnimationCurve walkFCurve;
    public AnimationCurve walkVCurve;
    public AnimationCurve jumpFCurve;
    public AnimationCurve jumpVCurve;
    public AnimationCurve rotationCurve;
    public AudioSource litFuse;

    [Title ("REFERENCES")]
    public EventListener eventListener;
    public Animator animator;
    public Transform model;
    public GameObject footPrintPrefab;
    public GameObject deathPrefab;
    public Sound deathSound;
    public Sound deathExplosion;
    public Sound stepSound;
    public Sound jumpSound;
    public Sound fallDeathSound;
    public Sound fallSound;
    public Sound penalitySound;
    public Sound disapointedSound;
    public Sound playerMoveSound;

    [Title ("DEBUG & RUNTIME")]
    public bool isMoving;

    float animationScale;
    bool fallOnMovement;
    bool lunchFall;
    bool isFalling;

    private void Awake()
    {
        g_onGameReboot += () => {
            animationScale = 1;
            fallOnMovement = false;
            lunchFall = false;
            isFalling = false;
            model.gameObject.SetActive(true);
        };
    }

    (Vector3Int, int) Cost(Vector3Int cell_in, Vector3Int direction)
    {
        int cost = 1000;
        if (cell_in.x + direction.x < 0 || cell_in.z + direction.z < 0 || cell_in.x + direction.x >= g_currentLevel.map.GetLength(0) || cell_in.z + direction.z >= g_currentLevel.map.GetLength(2))
        {
            return (cell_in + direction, cost);
        }
        CellDatas focus_level0 = g_currentLevel.map[cell_in.x + direction.x, cell_in.y + 0, cell_in.z + direction.z];
        CellDatas focus_level1 = g_currentLevel.map[cell_in.x + direction.x, cell_in.y + 1, cell_in.z + direction.z];
        CellDatas focus_level2 = g_currentLevel.map[cell_in.x + direction.x, cell_in.y + 2, cell_in.z + direction.z];
        Vector3Int jump = new Vector3Int(0, 0, 0);
        if (focus_level0 == CellDatas.Empty && focus_level1 == CellDatas.Empty)
        {
            cost = 1;
        }
        if ((focus_level0 == CellDatas.Solid || focus_level0 == CellDatas.Boulette) && focus_level1 == CellDatas.Empty && focus_level2 == CellDatas.Empty)
        {
            cost = 1;
            jump.y = 1;
        }
        return (cell_in + direction + jump, cost);
    }

    IEnumerator Start ()
    {
        eventListener.onEvent += Event;
        while (true) {
            while (isMoving && !isFalling) {

                Action action = Action.DontMove;
                //int perseption_width = 5;
                //int character_index_x = perseption_width/2;
                //CellDatas[,,] perseption = Perceive(perseption_width, 3, 2);
                Vector3Int current_index_cell = g_currentLevel.PositionToIndex(transform.position);
                Vector3Int focus_index_cell = g_currentLevel.PositionToIndex(transform.position + transform.forward);
                Vector3Int direction_forward = focus_index_cell - current_index_cell;
                Vector3Int direction_up = new Vector3Int(0, -1, 0);
                Vector3Int direction_right = new Vector3Int(
                    direction_forward.y * direction_up.z - direction_forward.z * direction_up.y,
                    direction_forward.z * direction_up.x - direction_forward.x * direction_up.z,
                    direction_forward.x* direction_up.y - direction_forward.y * direction_up.x);
                (Vector3Int next_cell_forward, int cost_forward) = Cost(current_index_cell, direction_forward);
                (Vector3Int next_cell_right, int cost_right) = Cost(current_index_cell, direction_right);
                (Vector3Int next_cell_right_forward, int cost_right_forward) = Cost(next_cell_right, direction_forward);
                (Vector3Int next_cell_left, int cost_left) = Cost(current_index_cell, -direction_right);
                (Vector3Int next_cell_left_forward, int cost_left_forward) = Cost(next_cell_left, direction_forward);
                int cost_path_forward = cost_forward;
                int cost_path_right = cost_right + cost_right_forward;
                int cost_path_left = cost_left + cost_left_forward;
                if (cost_path_right <= cost_path_forward && cost_path_right <= cost_path_left)
                {
                    Vector3Int next_cell = current_index_cell + direction_right;
                    action = g_currentLevel.map[next_cell.x, next_cell.y, next_cell.z] == CellDatas.Empty ? Action.Right : Action.JumpRight;
                }
                if (cost_path_left <= cost_path_forward && cost_path_left <= cost_path_right)
                {
                    Vector3Int next_cell = current_index_cell - direction_right;
                    action = g_currentLevel.map[next_cell.x, next_cell.y, next_cell.z] == CellDatas.Empty ? Action.Left : Action.JumpLeft;
                }
                if (cost_path_forward <= cost_path_right && cost_path_forward <= cost_path_left)
                {
                    Vector3Int next_cell = current_index_cell + direction_forward;
                    action = g_currentLevel.map[next_cell.x, next_cell.y, next_cell.z] == CellDatas.Empty ? Action.Forward : Action.JumpForward;
                }
                if (cost_path_forward >= 1000 && cost_path_right >= 1000 && cost_path_left >= 1000)
                {
                    action = Action.AboutFace;
                }
                if (current_index_cell.y == 0) {
                    action = Action.Fall;
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
                        break;
                    case Action.AboutFace:
                        transform.forward = -transform.forward;
                        disapointedSound.Play();
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

            Vector3Int indexBottom = g_currentLevel.PositionToIndex (end - Vector3.up);
            fallOnMovement = g_currentLevel.map [indexBottom.x, indexBottom.y, indexBottom.z] == CellDatas.Empty;

            Vector3Int cell = g_currentLevel.PositionToIndex(start);
            g_currentLevel.map[cell.x, cell.y, cell.z] = CellDatas.Empty;
            cell = g_currentLevel.PositionToIndex(end);
            g_currentLevel.map[cell.x, cell.y, cell.z] = CellDatas.Character;

            float duration = tickDuration * movementTickPercent;
            Vector3 df = (end - start).WithY(0).normalized;
            float dv = end.y - start.y;
            Vector3 simulatedPos = Vector3.zero;

            float percent = 0; while ((percent += Time.deltaTime / duration) < 1) {

                // Move 
                Vector3 lastPos = simulatedPos;
                simulatedPos = df * f.Evaluate (percent) + Vector3.up * dv * v.Evaluate (percent);
                model.forward = Vector3.Lerp(directionStart, directionEnd, rotationCurve.Evaluate(percent));
                transform.position += simulatedPos - lastPos;

                // Fall => lance la routine
                if (lunchFall) { Fall(indexBottom); lunchFall = false; fallOnMovement = false; }

                yield return null;
            }

            if (!isFalling) transform.position = end;
            model.forward = directionEnd;

            g_onPlayerChanged?.Invoke(end);
        }
    }

    void Fall(Vector3Int bottomIndex)
    {
        StartCoroutine(FallRoutine()); IEnumerator FallRoutine()
        {
            isFalling = true;
            animationScale = 0.1f;
            isMoving = false;
            Vector3 velocity = Vector3.zero;
            bool isDeath = true;
            Vector3 endPoint = Vector3.zero;

            // Chute mortelle ?
            for (int y = bottomIndex.y - 1; y > 0; y--) {
                if (g_currentLevel.map[bottomIndex.x, y, bottomIndex.z] == CellDatas.Solid || g_currentLevel.map[bottomIndex.x, y, bottomIndex.z] == CellDatas.Boulette) {
                    isDeath = false;
                    endPoint = g_currentLevel.GetCellCenter(bottomIndex.x, y, bottomIndex.z) + g_currentLevel.cellHeight / 2;
                    break;  
                }
            }

            // Mort de chute
            if (isDeath) {
                fallDeathSound.Play();
                g_gameCamera.StopMovement();

                float startFallTime = Time.time;
                float fallDuration = 1;
                while (Time.time - startFallTime < fallDuration) {
                    velocity -= Vector3.up * Time.deltaTime * 10;
                    transform.position += velocity * Time.deltaTime;
                    yield return null;
                }

                OnPlayerDeath();
            }

            // Pénalité de chute
            else {

                float height = g_currentLevel.GetCellCenter (bottomIndex.x, bottomIndex.y + 1, bottomIndex.z).y;
                float fallHeight = height - endPoint.y - g_currentLevel.cellSize / 2;
                float fallDuration = Mathf.Sqrt ((2 * fallHeight) / 10);

                float fallTime = 0;
                while ((fallTime += Time.deltaTime / fallDuration) < 1) {
                    velocity -= Vector3.up * Time.deltaTime * 10;
                    transform.position += velocity * Time.deltaTime;
                    yield return null;
                }

                animationScale = 1;
                transform.position = endPoint;
                if (fallHeight > 1) penalitySound.Play();

                float resetWaitTime = Mathf.CeilToInt (fallDuration / TickManager.TickDuration);
                yield return new WaitForSeconds(resetWaitTime);

                isFalling = false;
                isMoving = true;
            }
        }
    }

    public void Rotate ()
    {

    }

    public void StartMovement ()
    {
        isMoving = true;
        animationScale = 1;
        litFuse.Play();
    }

    public void StopMovement ()
    {
        isMoving = false;
        litFuse.Stop();
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
        if (eventName.Contains ("StepSound") && !isFalling) stepSound.Play();
        if (eventName.Contains ("StepPart") && !isFalling) {
            GameObject footPrint = Instantiate(footPrintPrefab);
            footPrint.transform.position = transform.position + Vector3.up * 0.05f;
            footPrint.transform.forward = model.forward;
            footPrint.SetActive(true);
            Destroy(footPrint, 12);
        }

        if (eventName == "Jump" && !isFalling) jumpSound.Play();
        if (eventName == "Fall" && fallOnMovement) lunchFall = true;
        if (eventName == "DeathExplosion") DeathExplosion();
        if (eventName == "PlayerMove") playerMoveSound.Play();
    }

    public void DeathByHit()
    {
        animator.SetTrigger("death");
        StopMovement();
        g_throwBullet.DisallowThrow();
        g_onPlayerDeath?.Invoke();
        deathSound.Play();
        g_gameCamera.StopMovement();
    }

    public void DeathExplosion ()
    {
        GameObject explosion = Instantiate (deathPrefab);
        explosion.transform.position = transform.position;
        Destroy(explosion, deathExplosionDuration);
        deathExplosion.Play();
        model.gameObject.SetActive(false);

        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            yield return new WaitForSeconds(rebootAfter);
            g_gameManager.RebootLevel();
        }
    }

    public void OnPlayerDeath ()
    {
        g_onPlayerDeath?.Invoke();
        g_gameManager.RebootLevel();
    }
}