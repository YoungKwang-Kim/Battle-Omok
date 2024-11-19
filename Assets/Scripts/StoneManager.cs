using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(BoardManager))]
public class StoneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject blackStonePrefab;
    [SerializeField] private GameObject whiteStonePrefab;
    [SerializeField] private GameObject selectStonePrefab;

    private BoardManager boardManager;
    private bool isBlack;
    private Vector3 targetPoint;
    private string lastBoardPosition;

    private void Awake()
    {
        boardManager = GetComponent<BoardManager>();

        if (blackStonePrefab == null)
        {
            Debug.LogError("Black Stone Prefab is not assigned!");
        }
        if (whiteStonePrefab == null)
        {
            Debug.LogError("White Stone Prefab is not assigned!");
        }
        if (selectStonePrefab == null)
        {
            Debug.LogError("Select Stone Prefab is not assigned!");
        }
    }

    private void Start()
    {
        if (blackStonePrefab == null || whiteStonePrefab == null || selectStonePrefab == null)
        {
            enabled = false;
            return;
        }

        // 마스터 클라이언트가 흑돌
        NetworkManager.Instance.OnGameReadyToStart += () => { isBlack = PhotonNetwork.IsMasterClient; };
        
        Debug.Log($"Player is {(isBlack ? "Black" : "White")}");
        GameState.SetTurn(isBlack);
        selectStonePrefab.SetActive(false);
    }

    private void Update()
    {
        if (GameState.IsGameOver || !GameState.IsMyTurn) return;

        UpdateStonePreview();
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceStone();
        }
    }

    private void UpdateStonePreview()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider != null && hit.collider.CompareTag("Board"))
        {
            targetPoint = new Vector3(
                hit.collider.transform.position.x,
                0.5f,
                hit.collider.transform.position.z
            );
            lastBoardPosition = hit.collider.name;
            selectStonePrefab.transform.position = targetPoint;
            selectStonePrefab.SetActive(true);
        }
        else
        {
            selectStonePrefab.SetActive(false);
        }
    }

    private void TryPlaceStone()
    {
        if (string.IsNullOrEmpty(lastBoardPosition)) return;

        string[] position = lastBoardPosition.Split('_');
        if (position.Length < 3) return;

        if (int.TryParse(position[1], out int x) && int.TryParse(position[2], out int y))
        {
            if (boardManager.IsEmptyPosition(x, y))
            {
                // RPC 호출 시 현재 플레이어의 isBlack 값도 함께 전달
                photonView.RPC(nameof(PlaceStoneRPC), RpcTarget.All, x, y, targetPoint.x, targetPoint.z, isBlack);
            }
        }
    }

    [PunRPC]
    private void PlaceStoneRPC(int x, int y, float posX, float posZ, bool isBlackStone)
    {
        Debug.Log($"Placing {(isBlackStone ? "Black" : "White")} stone at position ({x}, {y})");

        // Vector3 재구성
        Vector3 position = new Vector3(posX, 0.5f, posZ);

        // 돌 생성 (PhotonNetwork.Instantiate 대신 일반 Instantiate 사용)
        GameObject stonePrefab = isBlackStone ? blackStonePrefab : whiteStonePrefab;
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);

        // 보드 상태 업데이트
        int stoneType = isBlackStone ? 1 : 2;
        boardManager.SetStone(x, y, stoneType);

        // 턴 변경
        NetworkManager.Instance.ChangeTurn();

        // 승리 조건 체크
        CheckWinCondition(x, y, stoneType);
    }

    private void CheckWinCondition(int x, int y, int stoneType)
    {
        if (WinChecker.CheckWin(boardManager.Stones, x, y, stoneType, boardManager.Rows, boardManager.Cols))
        {
            string winner = stoneType == 1 ? "Black" : "White";
            photonView.RPC(nameof(GameOverRPC), RpcTarget.All, $"{winner} Wins");
        }
    }

    [PunRPC]
    private void GameOverRPC(string winner)
    {
        GameState.SetGameOver(winner);
    }
}