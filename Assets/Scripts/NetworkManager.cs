using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }
    public Action OnGameReadyToStart;

    private const string GAME_STARTED_PROP = "GameStarted";
    private const string CURRENT_TURN_PROP = "CurrentTurn"; // 현재 턴 플레이어의 ActorNumber를 저장

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room : {PhotonNetwork.CurrentRoom.Name}");
        CheckGameStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} joined the room");
        CheckGameStart();
    }

    private void CheckGameStart()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Game can start - both players joined");

            if (PhotonNetwork.IsMasterClient)
            {
                StartGame();
            }
        }
    }

    private void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(GAME_STARTED_PROP))
        {
            return;
        }

        // 게임 시작 상태와 첫 턴을 설정 (마스터 클라이언트의 ActorNumber로 시작)
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable()
        {
            { GAME_STARTED_PROP, true },
            { CURRENT_TURN_PROP, PhotonNetwork.LocalPlayer.ActorNumber }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        GameState.ResetGame();
        GameState.SetTurn(true); // 마스터 클라이언트가 선공
        OnGameReadyToStart?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GAME_STARTED_PROP))
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                GameState.ResetGame();
                GameState.SetTurn(false); // 마스터 클라이언트가 아닌 경우 후공
                OnGameReadyToStart?.Invoke();
            }
        }

        // 턴 변경 처리
        if (propertiesThatChanged.ContainsKey(CURRENT_TURN_PROP))
        {
            int currentTurnActorNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties[CURRENT_TURN_PROP];
            bool isMyTurn = currentTurnActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
            GameState.SetTurn(isMyTurn);
        }
    }

    // 턴을 변경하는 메서드
    public void ChangeTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var currentTurnActorNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties[CURRENT_TURN_PROP];

            // 다음 플레이어의 ActorNumber를 찾음
            int nextTurnActorNumber = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.ActorNumber != currentTurnActorNumber)
                {
                    nextTurnActorNumber = player.ActorNumber;
                    break;
                }
            }

            // 턴 업데이트
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable()
            {
                { CURRENT_TURN_PROP, nextTurnActorNumber }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }
}