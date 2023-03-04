using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle_Picko : MonoBehaviour
{

    public Transform picko;
    public Transform inPosition;
    public Transform outPosition;

    public int phaseDelta;
    [Range(0f, 1f)] public float tickPercentageIn = 0.5f;
    [Range(0f, 1f)] public float tickPercentageOut = 0.25f;

    int currentState;
    Vector3 velocity;

    private void Awake()
    {
        currentState = phaseDelta;
        TickManager.onTick += () => {
            currentState++;
            currentState = currentState % 4;
        };
    }

    void Update()
    {
        Vector3 target = currentState > 1 ? outPosition.position : inPosition.position;
        float tickPercent = currentState > 1 ? tickPercentageOut : tickPercentageIn;
        picko.transform.position = Vector3.SmoothDamp(picko.transform.position, target, ref velocity, TickManager.TickDuration * tickPercent);
    }
}