using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class CameraMovement : NetworkBehaviour
{
    public Camera playerCamera;

    void Start()
    {
        // 找到玩家预设中的相机
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            // 如果是本地玩家，则激活相机，否则禁用相机
            playerCamera.gameObject.SetActive(isLocalPlayer);
        }
    }
}
