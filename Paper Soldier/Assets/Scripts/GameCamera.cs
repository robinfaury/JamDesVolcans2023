using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent (typeof (Camera))]
public class GameCamera : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    public Player player;
    public Level level;
    private CinemachineTrackedDolly dolly;

    private void Start()
    {
        dolly = vCam.GetCinemachineComponent<CinemachineTrackedDolly>();

    }

    public void Update()
    {
        if (dolly != null)
        {
            Vector3 playerPath = level.endPoint.position - level.startPoint.position;
            Vector3 playerPos = player.transform.position - level.startPoint.position;
            float realRatio = playerPos.magnitude / playerPath.magnitude;

            Debug.Log(realRatio);
            dolly.m_PathPosition = realRatio; // Mathf.Lerp(dolly.m_PathPosition, realRatio, Time.smoothDeltaTime);
        }


    }





}