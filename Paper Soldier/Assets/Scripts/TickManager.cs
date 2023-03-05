using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class TickManager : MonoBehaviour
{

    [Range(0.1f, 2)] public float tickDuration = 0.5f;

    public static System.Action onTick;
    public static float TickDuration;

    float timeBank;
    public bool isTicking;

    void Update ()
    {
        if (isTicking) {
            timeBank += Time.deltaTime;
            while (timeBank > tickDuration) {
                onTick?.Invoke();
                timeBank -= tickDuration;
            }
            TickDuration = tickDuration;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) Time.timeScale = 1;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Time.timeScale = 2;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) Time.timeScale = 4;
        if (Keyboard.current.digit8Key.wasPressedThisFrame) g_player.DeathByHit();
        if (Keyboard.current.digit9Key.wasPressedThisFrame) g_gameManager.RebootLevel(true);
    }

    public void StartTicking ()
    {
        isTicking = true;
        timeBank = 0;
    }

    public void StopTicking ()
    {
        isTicking = false;
        timeBank = 0;
    }
}