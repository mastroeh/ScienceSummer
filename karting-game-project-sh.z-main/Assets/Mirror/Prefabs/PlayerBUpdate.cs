using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBUpdate : NetworkBehaviour
{
    public Transform positionTran;

    void Update()
    {
        if (positionTran != null)
        {
            // 更新位置和旋转以匹配 PositionTran
            transform.position = positionTran.position;
            transform.rotation = positionTran.rotation;

            // 如果是本地玩家，则将更新发送到服务器
            if (isLocalPlayer)
            {
                CmdUpdatePositionOnServer(transform.position, transform.rotation);
            }
        }
    }

    [Command]
    void CmdUpdatePositionOnServer(Vector3 newPosition, Quaternion newRotation)
    {
        // 服务器接收位置和旋转更新，并将其同步到所有客户端
        RpcUpdatePositionForOthers(newPosition, newRotation);
    }

    [ClientRpc]
    void RpcUpdatePositionForOthers(Vector3 updatedPosition, Quaternion updatedRotation)
    {
        // 如果不是本地玩家，则更新位置和旋转
        if (!isLocalPlayer)
        {
            transform.position = updatedPosition;
            transform.rotation = updatedRotation;
        }
    }
}
