using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;    

public class GameManager : MonoBehaviour
{

    [Title("BOUCLE")]
    public float transitionDelay = 4;

    [Title("REFERENCES")]
    public CanvasGroup canvasGroup;
    public Sound music;
    public Sound coucouSound;
    public List<Level> levels;

    [Title("REFERENCES GLOBALES")]
    public Player player;
    public TickManager tickManager;

    public static int g_currentLevelIndex;
    public static Level g_currentLevel;

    public static List<Level> g_levels;
    public static Player g_player;
    public static TickManager g_tickManager;
    public static GameManager g_gameManager;

    public static System.Action<Vector3> g_onPlayerChanged;
    public static bool g_isGamePlaying;

    void Awake()
    {
        g_player = player;
        g_tickManager = tickManager;
        g_currentLevelIndex = 0;
        g_gameManager = this;
        g_levels = levels;
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

            g_player.StartMovement();
            g_tickManager.StartTicking();
            LoadLevel(0);
            canvasGroup.alpha = 0;
            coucouSound.Play();

            yield return new WaitForEndOfFrame();
            g_isGamePlaying = true;
        }
    }

    public static void LoadLevel (int index)
    {
        if (g_currentLevel != null) g_onPlayerChanged -= g_currentLevel.OnPlayerPositionChanged;

        Level level = g_levels[index];
        g_currentLevel = level;
        Vector3 gridBottomPos = g_currentLevel.GetCellBottomAt (level.startPoint.position);
        g_player.transform.position = gridBottomPos;
        g_currentLevel.GenerateVoxel();

        g_onPlayerChanged += g_currentLevel.OnPlayerPositionChanged;
    }

    public void NextLevel ()
    {
        StartCoroutine(Routine()); IEnumerator Routine()
        {
            g_player.StopMovement();
            g_tickManager.StopTicking();
            FadeFromTo(0, 1, transitionDelay);
            yield return new WaitForSeconds(transitionDelay);
            LoadLevel(g_currentLevelIndex++);
            FadeFromTo(1, 0, transitionDelay);
            yield return new WaitForSeconds(transitionDelay);

            g_player.StartMovement();
            g_tickManager.StartTicking();
            canvasGroup.alpha = 0;
            coucouSound.Play();

            yield return new WaitForEndOfFrame();
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