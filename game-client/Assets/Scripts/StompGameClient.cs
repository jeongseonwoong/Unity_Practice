using System;
using System.Text;
using UnityEngine;
using WebSocketSharp;   // websocket-sharp DLL 추가 후 사용 가능

// ===== DTO 영역 =====

[Serializable]
public class OperationDto
{
    public string type = "MOVE"; // OperationType.MOVE
    public long playerId;
    public double x;
    public double y;
    public double z;
}

[Serializable]
public class MoveEventPayloadDto
{
    public OperationDto operation;
}

// ===== STOMP 클라이언트 MonoBehaviour =====

public class StompGameClient : MonoBehaviour
{
    [Header("Server Settings")]
    public string serverUrl = "ws://localhost:9000/ws-native"; // 나중에 실제 서버 주소로 수정
    public string roomId = "1";                                // 방 ID
    public long playerId = 1L;                                 // 플레이어 ID (각 클라마다 다르게)

    [Header("Move Send Settings")]
    public float sendInterval = 0.1f; // 몇 초마다 위치를 보낼지 (0.1초 = 1초에 10번)

    private WebSocket ws;
    private bool stompConnected = false;
    private float sendTimer = 0f;

    private const string StompVersion = "1.2";
    private int subscribeId = 0;

    void Start()
    {
        Connect();
    }

    void Update()
    {
        if (stompConnected)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= sendInterval)
            {
                sendTimer = 0f;
                SendCurrentPosition();
            }
        }
    }

    void OnDestroy()
    {
        Disconnect();
    }

    // ---------------------------
    // WebSocket + STOMP 연결
    // ---------------------------
    private void Connect()
    {
        ws = new WebSocket(serverUrl);

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("[STOMP] WebSocket connected");
            SendConnectFrame();
        };

        ws.OnMessage += (sender, e) =>
        {
            if (e.IsText)
            {
                HandleStompFrame(e.Data);
            }
            else if (e.IsBinary)
            {
                Debug.LogWarning("[STOMP] Binary frame ignored");
            }
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("[STOMP] Error: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("[STOMP] Closed: " + e.Reason);
            stompConnected = false;
        };

        ws.Connect();
    }

    private void Disconnect()
    {
        try
        {
            if (ws != null)
            {
                if (ws.IsAlive)
                {
                    ws.Close();
                }
                ws = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[STOMP] Disconnect error: " + e);
        }
    }

    // ---------------------------
    // STOMP Frame 구성/전송
    // ---------------------------

    private void SendConnectFrame()
    {
        var sb = new StringBuilder();
        sb.Append("CONNECT\n");
        sb.Append("accept-version:").Append(StompVersion).Append("\n");
        sb.Append("host:localhost\n");
        sb.Append("\n");
        sb.Append('\0');

        ws.Send(sb.ToString());
        Debug.Log("[STOMP] CONNECT frame sent");
    }

    private void SendSubscribeFrame()
    {
        subscribeId++;
        var sb = new StringBuilder();
        sb.Append("SUBSCRIBE\n");
        sb.Append("id:sub-").Append(subscribeId).Append("\n");
        sb.Append("destination:/topic/").Append(roomId).Append("\n");
        sb.Append("\n");
        sb.Append('\0');

        ws.Send(sb.ToString());
        Debug.Log("[STOMP] SUBSCRIBE frame sent, roomId=" + roomId);
    }

    /// <summary>
    /// 이 컴포넌트가 붙어있는 오브젝트의 위치를 MOVE 이벤트로 전송
    /// </summary>
    private void SendCurrentPosition()
    {
        if (!stompConnected || ws == null || !ws.IsAlive)
            return;

        Vector3 pos = transform.position;

        var op = new OperationDto
        {
            type = "MOVE",
            playerId = playerId,
            x = pos.x,
            y = pos.y,
            z = pos.z
        };
        var payload = new MoveEventPayloadDto
        {
            operation = op
        };

        string body = JsonUtility.ToJson(payload);

        var sb = new StringBuilder();
        sb.Append("SEND\n");
        sb.Append("destination:/app/").Append(roomId).Append("\n");
        sb.Append("content-type:application/json\n");
        sb.Append("content-length:").Append(Encoding.UTF8.GetByteCount(body)).Append("\n");
        sb.Append("\n");
        sb.Append(body);
        sb.Append('\0');

        ws.Send(sb.ToString());
        // Debug.Log("[STOMP] SEND MOVE: " + body);
    }

    // ---------------------------
    // STOMP Frame 수신 처리
    // ---------------------------

    private void HandleStompFrame(string frame)
    {
        frame = frame.TrimEnd('\0');

        int firstLineEnd = frame.IndexOf('\n');
        if (firstLineEnd < 0)
        {
            Debug.LogWarning("[STOMP] invalid frame (no command line): " + frame);
            return;
        }

        string command = frame.Substring(0, firstLineEnd).Trim();
        string rest = frame.Substring(firstLineEnd + 1);

        int emptyLineIndex = rest.IndexOf("\n\n", StringComparison.Ordinal);
        string headersPart;
        string bodyPart;

        if (emptyLineIndex >= 0)
        {
            headersPart = rest.Substring(0, emptyLineIndex);
            bodyPart = rest.Substring(emptyLineIndex + 2);
        }
        else
        {
            headersPart = rest;
            bodyPart = "";
        }

        switch (command)
        {
            case "CONNECTED":
                Debug.Log("[STOMP] CONNECTED");
                stompConnected = true;
                SendSubscribeFrame();
                break;

            case "MESSAGE":
                HandleMessageFrame(headersPart, bodyPart);
                break;

            case "ERROR":
                Debug.LogError("[STOMP] ERROR frame: " + bodyPart);
                break;

            default:
                Debug.Log("[STOMP] Frame: " + command + " (ignored)");
                break;
        }
    }

    private void HandleMessageFrame(string headers, string body)
    {
        try
        {
            var payload = JsonUtility.FromJson<MoveEventPayloadDto>(body);
            if (payload?.operation == null)
            {
                Debug.LogWarning("[STOMP] MESSAGE payload has no operation");
                return;
            }

            var op = payload.operation;
            Debug.Log($"[STOMP] MESSAGE MOVE: playerId={op.playerId}, pos=({op.x},{op.y},{op.z})");

            // TODO: 여기서 op.playerId 기준으로 다른 플레이어 오브젝트 찾아서 위치 갱신하면 됨.
        }
        catch (Exception e)
        {
            Debug.LogError("[STOMP] Failed to parse MESSAGE body: " + e + "\nbody=" + body);
        }
    }
}
