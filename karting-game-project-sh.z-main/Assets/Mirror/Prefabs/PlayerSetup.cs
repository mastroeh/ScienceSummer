using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    public Transform positionTran; // 指向 PositionTran 的引用
    public float heightAbovePlayer = 1.0f; // 玩家A正上方的高度

    void Update()
    {
        if (positionTran != null)
        {
            // 将 positionTran 设置在玩家A正上方一定高度
            positionTran.position = transform.position + Vector3.up * heightAbovePlayer;
            // 可以选择保持 positionTran 的旋转与玩家A一致，或者设置为其他需要的旋转
            positionTran.rotation = transform.rotation;
        }
    }
}

