using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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