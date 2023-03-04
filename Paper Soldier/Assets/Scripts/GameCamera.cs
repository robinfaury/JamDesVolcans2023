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

    public void SetVCam (CinemachineVirtualCamera vCam)
    {
        this.vCam = vCam;
        dolly = vCam.GetCinemachineComponent<CinemachineTrackedDolly>();
        vCam.gameObject.SetActive(true);
    }

    public void Update()
    {
        if (dolly != null)
        {
            Vector3 playerPath = g_currentLevel.endPoint.position - g_currentLevel.startPoint.position;
            Vector3 playerPos = g_player.transform.position - g_currentLevel.startPoint.position;
            float realRatio = playerPos.magnitude / playerPath.magnitude;
            dolly.m_PathPosition = realRatio; // Mathf.Lerp(dolly.m_PathPosition, realRatio, Time.smoothDeltaTime);
        }
    }
}