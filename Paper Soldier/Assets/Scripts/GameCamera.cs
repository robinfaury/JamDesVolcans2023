using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using static GameManager;

[RequireComponent (typeof (Camera))]
public class GameCamera : MonoBehaviour
{
    CinemachineVirtualCamera vCam;
    CinemachineTrackedDolly dolly;

    bool isMoving;
    float targetPos;

    private void Awake()
    {
        g_onGameReboot += () => {
            targetPos = 0;
            if (dolly != null) dolly.m_PathPosition = 0;
        };
    }

    public void SetVCam (CinemachineVirtualCamera vCam)
    {
        this.vCam = vCam;
        dolly = vCam.GetCinemachineComponent<CinemachineTrackedDolly>();
        vCam.gameObject.SetActive(true);
    }

    public void Update()
    {
        if (dolly != null) {
            if (isMoving) {
                Vector3 playerPath = g_currentLevel.endPoint.position - g_currentLevel.startPoint.position;
                Vector3 playerPos = g_player.transform.position - g_currentLevel.startPoint.position;
                float realRatio = playerPos.magnitude / playerPath.magnitude;
                targetPos = realRatio;
            }

            dolly.m_PathPosition = Mathf.Lerp(dolly.m_PathPosition, targetPos, Time.smoothDeltaTime / 10);
        }
    }

    public void StartMovement ()
    {
        isMoving = true;
    }

    public void StopMovement ()
    {
        isMoving = false;
    }
}