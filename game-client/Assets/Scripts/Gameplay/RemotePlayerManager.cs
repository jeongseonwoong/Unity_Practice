using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerManager : MonoBehaviour
{
    public GameObject remotePlayerPrefab;
    private Dictionary<long, GameObject> remotePlayers = new();

    public long localPlayerId = 1L; // 내 플레이어 ID (StompGameClient랑 맞추기)

    public void ApplyRemoteMove(long playerId, double x, double y, double z)
    {
        if (playerId == localPlayerId)
            return; // 내가 보낸 거면 무시

        if (!remotePlayers.TryGetValue(playerId, out var playerObj) || playerObj == null)
        {
            // 처음 보는 플레이어면 새로 생성
            playerObj = Instantiate(remotePlayerPrefab, Vector3.zero, Quaternion.identity);
            remotePlayers[playerId] = playerObj;
        }

        playerObj.transform.position = new Vector3((float)x, (float)y, (float)z);
    }
}
