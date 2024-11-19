using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }
    public Action OnGameReadyToStart;

    private const string GAME_STARTED_PROP = "GameStarted";
    private const string CURRENT_TURN_PROP = "CurrentTurn"; // ���� �� �÷��̾��� ActorNumber�� ����

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

        // ���� ���� ���¿� ù ���� ���� (������ Ŭ���̾�Ʈ�� ActorNumber�� ����)
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable()
        {
            { GAME_STARTED_PROP, true },
            { CURRENT_TURN_PROP, PhotonNetwork.LocalPlayer.ActorNumber }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        GameState.ResetGame();
        GameState.SetTurn(true); // ������ Ŭ���̾�Ʈ�� ����
        OnGameReadyToStart?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GAME_STARTED_PROP))
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                GameState.ResetGame();
                GameState.SetTurn(false); // ������ Ŭ���̾�Ʈ�� �ƴ� ��� �İ�
                OnGameReadyToStart?.Invoke();
            }
        }

        // �� ���� ó��
        if (propertiesThatChanged.ContainsKey(CURRENT_TURN_PROP))
        {
            int currentTurnActorNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties[CURRENT_TURN_PROP];
            bool isMyTurn = currentTurnActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
            GameState.SetTurn(isMyTurn);
        }
    }

    // ���� �����ϴ� �޼���
    public void ChangeTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var currentTurnActorNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties[CURRENT_TURN_PROP];

            // ���� �÷��̾��� ActorNumber�� ã��
            int nextTurnActorNumber = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.ActorNumber != currentTurnActorNumber)
                {
                    nextTurnActorNumber = player.ActorNumber;
                    break;
                }
            }

            // �� ������Ʈ
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable()
            {
                { CURRENT_TURN_PROP, nextTurnActorNumber }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }
}