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

    // �׸��� �� �����
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
                // �� ��ġ
                Vector3 cellPosition = new Vector3(x * cellSize, -0.5f, y * cellSize);

                // �� ����
                GameObject cell = Instantiate(CellPrefab, cellPosition, Quaternion.identity);

                // ���� �׸��� �迭�� ����
                board[x, y] = cell;
                stones[x, y] = empty;

                // �� �̸�
                cell.name = $"Cell_{x}_{y}";
                cell.tag = "Board";
                cell.transform.parent = this.transform;
            }
        }
    }

    // ���� �� �� �ִ� ������ �Ǻ��ϴ� �Լ�
    bool IsVailablePosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

    // �� ���� �Լ�
    void CreateStone(int x, int y, Vector3 targetPoint)
    {
        if (!IsVailablePosition(x, y))
        {
            Debug.LogError("You Can't lay stone here.");
            return;
        }
        // ���� ������
        if (stones[x, y] == empty)
        {
            // �뵹�� ��
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
            // �鵹�� ��
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

    // ���� ���� ��ġ �ð�ȭ
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

    // �¸� ���� üũ �Լ�
    private void CheckWin(int x, int y, int stoneColor)
    {
        // 4���� ����: ����, ����, �밢�� (\), ���밢�� (/)
        int[][] directions = new int[][] {
            new int[] { 1, 0 }, // ����
            new int[] { 0, 1 }, // ����
            new int[] { 1, 1 }, // �밢�� (\)
            new int[] { 1, -1 } // ���밢�� (/)
        };

        foreach (var dir in directions)
        {
            int count = 1;

            // ���� ���� üũ
            count += CountStonesInDirection(x, y, dir[0], dir[1], stoneColor);

            // �ݴ� ���� üũ
            count += CountStonesInDirection(x, y, -dir[0], -dir[1], stoneColor);

            // 5�� ���� Ȯ��
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

    // �־��� �������� ���� �� ������ ī��Ʈ�ϴ� �Լ�
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
