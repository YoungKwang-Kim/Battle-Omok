using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button resetButton;

    private void Start()
    {
        GameState.OnTurnChanged += UpdateTurnText;
        GameState.OnGameOver += ShowGameOver;

        gameOverText.enabled = false;
        startButton.enabled = false;
        resetButton.enabled = false;
    }

    private void OnDestroy()
    {
        GameState.OnTurnChanged -= UpdateTurnText;
        GameState.OnGameOver -= ShowGameOver;
    }

    void UpdateTurnText(bool isMyturn)
    {
        turnText.text = isMyturn ? "Your Turn" : "Opponent's Turn";
    }

    void ShowGameOver(string winner)
    {
        gameOverText.enabled = true;
        gameOverText.text = winner;

        resetButton.enabled = true;
    }

    public void ResetGame()
    {
        GameState.ResetGame();
        resetButton.enabled = false;
    }
}
