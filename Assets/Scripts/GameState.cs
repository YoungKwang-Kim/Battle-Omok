using System;

public class GameState
{
    public static event Action<bool> OnTurnChanged;
    public static event Action<string> OnGameOver;

    private static bool isGameOver;
    private static bool isMyTurn;

    public static bool IsGameOver => isGameOver;
    public static bool IsMyTurn => isMyTurn;

    public static void SetTurn(bool myTurn)
    {
        isMyTurn = myTurn;
        OnTurnChanged?.Invoke(myTurn);
    }

    public static void SetGameOver(string winner)
    {
        isGameOver = true;
        OnGameOver?.Invoke(winner);
    }

    public static void ResetGame()
    {
        isGameOver = false;
        isMyTurn = false;
    }
}
