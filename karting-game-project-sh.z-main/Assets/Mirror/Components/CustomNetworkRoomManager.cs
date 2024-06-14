using UnityEngine;
using Mirror;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    public GameObject playerRole1Prefab; // 第一个角色的预制件
    public GameObject playerRole2Prefab; // 第二个角色的预制件

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        // 使用基类的默认实现来创建房间玩家
        return base.OnRoomServerCreateRoomPlayer(conn);
    }


    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        // 根据连接类型选择角色预制件
        GameObject playerRolePrefab = (conn == NetworkServer.localConnection) ? playerRole1Prefab : playerRole2Prefab;

        Transform startPos = GetStartPosition();
        GameObject gamePlayer = startPos != null
            ? Instantiate(playerRolePrefab, startPos.position, startPos.rotation)
            : Instantiate(playerRolePrefab);

        gamePlayer.name = $"{playerRolePrefab.name} [connId={conn.connectionId}]";

        // 返回游戏玩家对象
        return gamePlayer;
    }
}
