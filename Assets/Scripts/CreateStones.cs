using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class CreateStones : MonoBehaviour
{
    [Header("Grid")]
    public int rows;
    public int cols;

    private const int empty = 0;
    private const int black = 1;
    private const int white = 2;

    public float cellSize = 1.0f;
    public GameObject CellPrefab;
    private GameObject[,] board;
    private int[,] stones;

    [Header("Stone")]
    [SerializeField]
    private GameObject blackStone;
    [SerializeField]
    private GameObject whiteStone;
    [SerializeField]
    private GameObject opacityStone;

    private Vector3 targetPoint = Vector3.zero;
    private string lastBoardPosition = string.Empty;

    private bool isBlack;
    private bool isWhite;

    [SerializeField]
    private TextMeshProUGUI gameoverText;

    private void Start()
    {
        CreateBoard(rows, cols);
    }

    void Update()
    {
        PlaceStone();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceStone();
        }
    }

    void Init()
    {
        isBlack = true;
        isWhite = false;
        opacityStone.SetActive(false);
        board = new GameObject[rows, cols];
        stones = new int[rows, cols];
        gameoverText.enabled = false;
    }

    // 그리드 맵 만들기
    void CreateBoard(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            Debug.LogError("Rows and Lows must be bigger than 0.");
        }
        Init();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                // 셀 위치
                Vector3 cellPosition = new Vector3(x * cellSize, -0.5f, y * cellSize);

                // 셀 생성
                GameObject cell = Instantiate(CellPrefab, cellPosition, Quaternion.identity);

                // 셀을 그리드 배열에 저장
                board[x, y] = cell;
                stones[x, y] = empty;

                // 셀 이름
                cell.name = $"Cell_{x}_{y}";
                cell.tag = "Board";
                cell.transform.parent = this.transform;
            }
        }
    }

    // 돌을 둘 수 있는 곳인지 판별하는 함수
    bool IsVailablePosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

    // 돌 놓는 함수
    void CreateStone(int x, int y, Vector3 targetPoint)
    {
        if (!IsVailablePosition(x, y))
        {
            Debug.LogError("You Can't lay stone here.");
            return;
        }
        // 돌이 없으면
        if (stones[x, y] == empty)
        {
            // 흙돌일 때
            if (isBlack)
            {
                GameObject stone = Instantiate(blackStone, targetPoint, Quaternion.identity);
                isWhite = true;
                isBlack = false;
                opacityStone.SetActive(false);

                stones[x, y] = black;

                Debug.Log($"Black stone is {x}, {y}");

                CheckWin(x, y, black);
            }
            // 백돌일 때
            else if (isWhite)
            {
                GameObject stone = Instantiate(whiteStone, targetPoint, Quaternion.identity);
                isWhite = false;
                isBlack = true;
                opacityStone.SetActive(false);

                stones[x, y] = white;

                Debug.Log($"White stone is {x}, {y}");

                CheckWin(x, y, white);
            }
        }
        else
        {
            Debug.Log("Stone is already here");
        }
    }

    // 돌을 놓을 위치 시각화
    void PlaceStone()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Board"))
        {
            targetPoint = new Vector3(hit.collider.transform.position.x, 0, hit.collider.transform.position.z);
            lastBoardPosition = hit.collider.name;
            opacityStone.transform.position = targetPoint;
            opacityStone.SetActive(true);

            if (hit.collider.CompareTag("Out") || hit.collider.CompareTag("Stone"))
            {
                opacityStone.SetActive(false);
            }
        }
    }

    void TryPlaceStone()
    {
        string[] nameSplit = lastBoardPosition.Split("_");
        if (nameSplit.Length >= 3)
        {
            CreateStone(int.Parse(nameSplit[1]), int.Parse(nameSplit[2]), targetPoint);
        }
    }

    // 승리 조건 체크 함수
    private void CheckWin(int x, int y, int stoneColor)
    {
        // 4가지 방향: 가로, 세로, 대각선 (\), 역대각선 (/)
        int[][] directions = new int[][] {
            new int[] { 1, 0 }, // 가로
            new int[] { 0, 1 }, // 세로
            new int[] { 1, 1 }, // 대각선 (\)
            new int[] { 1, -1 } // 역대각선 (/)
        };

        foreach (var dir in directions)
        {
            int count = 1;

            // 한쪽 방향 체크
            count += CountStonesInDirection(x, y, dir[0], dir[1], stoneColor);

            // 반대 방향 체크
            count += CountStonesInDirection(x, y, -dir[0], -dir[1], stoneColor);

            // 5개 연속 확인
            if (count >= 5 && stoneColor == 1)
            {
                gameoverText.enabled = true;
                gameoverText.text = "Black win";
            }
            else if (count >= 5 && stoneColor == 2)
            {
                gameoverText.enabled = true;
                gameoverText.text = "White win";
            }
        }
    }

    // 주어진 방향으로 같은 돌 개수를 카운트하는 함수
    private int CountStonesInDirection(int x, int y, int dx, int dy, int stoneColor)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (nx >= 0 && ny >= 0 && nx < 19 && ny < 19 && stones[nx, ny] == stoneColor)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }
}
