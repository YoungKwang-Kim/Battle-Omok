using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    private void Start()
    {
        CreateBoard(rows, cols);
    }

    // Update is called once per frame
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

                Debug.Log($"black stone is {x}, {y}");
            }
            // �鵹�� ��
            else if (isWhite)
            {
                GameObject stone = Instantiate(whiteStone, targetPoint, Quaternion.identity);
                isWhite = false;
                isBlack = true;
                opacityStone.SetActive(false);

                stones[x, y] = white;

                Debug.Log($"white Stone is {x}, {y}");
            }

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
}
