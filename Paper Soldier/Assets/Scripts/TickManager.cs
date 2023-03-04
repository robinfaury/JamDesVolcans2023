using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{

    [Range(0.1f, 1)] public float tickDuration = 0.5f;

    public static System.Action onTick;
    public static float TickDuration;

    float timeBank;

    private void Start()
    {
        timeBank = 0;
    }

    void Update ()
    {
        timeBank += Time.deltaTime;
        while (timeBank > tickDuration) {
            onTick?.Invoke();
            timeBank -= tickDuration;
        }
        TickDuration = tickDuration;
    }
}