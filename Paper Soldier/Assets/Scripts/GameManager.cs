using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;    

public class GameManager : MonoBehaviour
{

    [Title("BOUCLE")]
    public float transitionDelay = 4;
    public int defautlLevelIndex = 0;

    [Title("REFERENCES")]
    public CanvasGroup canvasGroup;
    public GameObject confettiFinishLevel;
    public Sound music;
    public Sound levelSuccess;
    public Sound bastonSound;
    public Sound playerSuccess;
    public List<Level> levels;

    [Title("REFERENCES GLOBALES")]
    public Player player;
    public TickManager tickManager;
    public GameCamera gameCamera;
    public ThrowBullet throwBullet;

    public static int g_currentLevelIndex;
    public static Level g_currentLevel;
    public static GameCamera g_gameCamera;

    public static List<Level> g_levels;
    public static Player g_player;
    public static TickManager g_tickManager;
    public static GameManager g_gameManager;
    public static ThrowBullet g_throwBullet;

    public static Score g_score = new Score();
    public static bool g_isGamePlaying;

    public static System.Action<Vector3> g_onPlayerChanged;
    public static System.Action g_onPlayerDeath;
    public static System.Action g_onGameReboot;
    public static System.Action g_onLevelChanged;

    void Awake()
    {
        g_player = player;
        g_tickManager = tickManager;
        g_currentLevelIndex = defautlLevelIndex;
        g_gameManager = this;
        g_levels = levels;
        g_gameCamera = gameCamera;
        g_throwBullet = throwBullet;
        if (g_throwBullet == null)
            g_throwBullet = FindObjectOfType<ThrowBullet>();
        g_score.NewGame();
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame ()
    {
        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            FadeFromTo(1, 0, transitionDelay);
            music.Play();

            yield return new WaitForSeconds(transitionDelay);

            g_tickManager.StartTicking();
            g_player.StartMovement();

            LoadLevel(defautlLevelIndex);
            canvasGroup.alpha = 0;
            bastonSound.Play();

            yield return new WaitForEndOfFrame();
            g_isGamePlaying = true;
        }
    }

    public static void LoadLevel (int index)
    {
        if (g_currentLevel != null) g_onPlayerChanged -= g_currentLevel.OnPlayerPositionChanged;

        Level level = g_levels[index];
        g_currentLevel = level;
        g_currentLevel.GenerateVoxel();
        g_gameCamera.SetVCam(g_currentLevel.virtualCamera);
        g_gameCamera.StartMovement();
        g_throwBullet.AllowThrow();

        Vector3 gridBottomPos = g_currentLevel.GetCellBottomAt (level.startPoint.position);
        g_player.transform.position = gridBottomPos;
        g_player.transform.forward = (level.endPoint.position - level.startPoint.position).WithY(0).normalized;

        g_onPlayerChanged += g_currentLevel.OnPlayerPositionChanged;
        g_onLevelChanged?.Invoke();

        g_throwBullet.Reset();
    }

    public void RebootLevel (bool skipTransi = false)
    {
        StartCoroutine(Routine()); IEnumerator Routine()
        {
            g_tickManager.StopTicking();
            g_player.StopMovement();
            g_throwBullet.DisallowThrow();

            if (!skipTransi) FadeFromTo(0, 1, transitionDelay);
            if (!skipTransi) yield return new WaitForSeconds(transitionDelay);

            g_onGameReboot?.Invoke();

            LoadLevel(g_currentLevelIndex);
            if (!skipTransi) yield return new WaitForSeconds(transitionDelay * 2);
            if (!skipTransi) FadeFromTo(1, 0, transitionDelay);

            g_player.StartMovement();
            g_tickManager.StartTicking();
            g_throwBullet.AllowThrow();
            canvasGroup.alpha = 0;
            bastonSound.Play();
            g_isGamePlaying = true;
        }
    }

    public void NextLevel ()
    {
        g_score.DebugScore();

        StartCoroutine(Routine()); IEnumerator Routine()
        {
            GameObject confetti = Instantiate (confettiFinishLevel);
            confetti.transform.position = g_currentLevel.endPoint.position;
            confetti.transform.forward = -g_player.transform.forward;
            levelSuccess.Play();
            playerSuccess.Play();
            Destroy(confetti, 5);

            g_tickManager.StopTicking();
            g_player.StopMovement();
            g_throwBullet.DisallowThrow();

            yield return new WaitForSeconds(transitionDelay);
            FadeFromTo(0, 1, transitionDelay);
            yield return new WaitForSeconds(transitionDelay);

            g_currentLevelIndex++;
            LoadLevel(g_currentLevelIndex);
            yield return new WaitForSeconds(transitionDelay);

            FadeFromTo(1, 0, transitionDelay);
            g_player.StartMovement();
            g_tickManager.StartTicking();
            g_throwBullet.AllowThrow();
            canvasGroup.alpha = 0;
            bastonSound.Play();
            g_isGamePlaying = true;
        }
    }

    void FadeFromTo(float from, float to, float duration)
    {
        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            float percent = 0; while ((percent += Time.deltaTime / duration) < 1) {
                float value = Mathf.Lerp (from, to, percent);
                canvasGroup.alpha = value;
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}