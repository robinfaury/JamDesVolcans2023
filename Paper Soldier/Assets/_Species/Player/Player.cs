using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using static GameManager;
using UnityEngine.TextCore.Text;

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

    IEnumerator Start ()
    {
        eventListener.onEvent += Event;
        while (true) {
            while (isMoving && !isFalling) {

                Action action = Action.DontMove;
                int perseption_width = 5;
                int character_index_x = perseption_width/2;
                CellDatas[,,] perseption = Perceive(perseption_width, 2, 2);
                Vector3Int current_index_cell = g_currentLevel.PositionToIndex(transform.position);

                if (current_index_cell.y == 0) {
                    action = Action.Fall;
                }
                else {
                    // Déplacement de la logique de fall dans le movement
                    // Pour enchainer la chute directement après le déplacement
                    /*
                    if (g_currentLevel.map[current_index_cell.x, current_index_cell.y - 1, current_index_cell.z] == CellDatas.Empty) {
                        action = Action.Fall;
                    }
                    */
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
                    else if ((perseption[character_index_x - 1, 1, 1] == CellDatas.Solid || perseption[character_index_x - 1, 1, 1] == CellDatas.Boulette) && (perseption[character_index_x - 1, 0, 0] == CellDatas.Solid || perseption[character_index_x - 1, 0, 0] == CellDatas.Boulette))
                    {
                        action = Action.JumpLeft;
                    }
                    else if ((perseption[character_index_x + 1, 1, 1] == CellDatas.Solid || perseption[character_index_x + 1, 1, 1] == CellDatas.Boulette) && (perseption[character_index_x + 1, 0, 0] == CellDatas.Solid || perseption[character_index_x + 1, 0, 0] == CellDatas.Boulette))
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
                        break;
                    case Action.AboutFace:
                        transform.forward = -transform.forward;
                        penalitySound.Play();
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

            Vector3Int index = g_currentLevel.PositionToIndex (end - Vector3.up);
            fallOnMovement = g_currentLevel.map [index.x, index.y, index.z] == CellDatas.Empty;

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
                if (lunchFall) { Fall(index); lunchFall = false; fallOnMovement = false; }

                yield return null;
            }

            if (!isFalling) transform.position = end;
            model.forward = directionEnd;

            g_onPlayerChanged?.Invoke(end);
        }
    }

    void Fall(Vector3Int index)
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
            for (int y = index.y - 1; y > 0; y--) {
                if (g_currentLevel.map[index.x, y, index.z] == CellDatas.Solid || g_currentLevel.map[index.x, y, index.z] == CellDatas.Boulette) {
                    isDeath = false;
                    endPoint = g_currentLevel.GetCellBottomAt(new Vector3 (index.x, y, index.z));
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
                fallSound.Play();
                float fallHeight = index.y - endPoint.y - g_currentLevel.cellSize / 4;
                float fallDuration = Mathf.Sqrt ((2 * fallHeight) / 10);
                float fallTime = 0;
                while ((fallTime += Time.deltaTime / fallDuration) < 1) {
                    velocity -= Vector3.up * Time.deltaTime * 10;
                    transform.position += velocity * Time.deltaTime;
                    yield return null;
                }

                animationScale = 1;
                transform.position = g_currentLevel.GetCellBottomAt(transform.position) + Vector3.up;
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
        if (eventName.Contains ("StepSound") && !isFalling) stepSound.Play();
        if (eventName.Contains ("StepPart") && !isFalling) {
            GameObject footPrint = Instantiate(footPrintPrefab);
            footPrint.transform.position = transform.position + Vector3.up * 0.05f;
            footPrint.transform.forward = transform.forward;
            footPrint.SetActive(true);
            Destroy(footPrint, 12);
        }

        if (eventName == "Jump" && !isFalling) jumpSound.Play();
        if (eventName == "Fall" && fallOnMovement) lunchFall = true;
        if (eventName == "DeathExplosion") DeathExplosion();
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