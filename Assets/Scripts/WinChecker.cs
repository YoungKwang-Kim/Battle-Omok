public class WinChecker
{
    public static bool CheckWin(int[,] board, int x, int y, int stoneType, int rows, int cols)
    {
        // 4가지 방향: 가로, 세로, 대각선(\), 역대각선(/)
        int[][] directions = new int[][]
        {
            new int[] { 1, 0 },
            new int[] { 0, 1 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };

        foreach (var dir in directions)
        {
            int count = 1;
            count += CountStonesInDirection(board, x, y, dir[0], dir[1], stoneType, rows, cols);
            count += CountStonesInDirection(board, x, y, -dir[0], -dir[1], stoneType, rows, cols);

            if (count >= 5) return true;
        }

        return false;
    }

    private static int CountStonesInDirection(int[,] board, int x, int y, int dx, int dy,
        int stoneType, int rows, int cols)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (nx >= 0 && ny >= 0 && nx < rows && ny < cols && board[nx, ny] == stoneType)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }
}
