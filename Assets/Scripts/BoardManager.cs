using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int rows = 10;
    [SerializeField] private int cols = 10;
    [SerializeField] private float cellSize = 1f;

    private GameObject[,] board;
    private int[,] stones;

    public int[,] Stones => stones;
    public int Rows => rows;
    public int Cols => cols;

    private void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        if (rows <= 0 || cols <= 0) return;

        board = new GameObject[rows, cols];
        stones = new int[rows, cols];

        for (int x = 0; x < rows; x++) 
        { 
            for (int y = 0; y < cols; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0f, y * cellSize);
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                board[x, y] = cell;
                stones[x, y] = 0;
                cell.name = $"Cell_{x}_{y}";
                cell.tag = "Board";
                cell.transform.parent = this.transform;
            }
        }
    }

    // 돌을 둘 수 있는 곳인지 판별하는 함수
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

    public bool IsEmptyPosition(int x, int y)
    {
        return IsValidPosition(x, y) && stones[x, y] == 0;
    }

    public void SetStone(int x, int y, int stoneType)
    {
        if (IsValidPosition(x, y))
        {
            stones[x, y] = stoneType;
        }
    }

}
