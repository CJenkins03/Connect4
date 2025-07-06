using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EndScreenUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Color winColour;
    [SerializeField] private Color looseColour;
    [SerializeField] private Color tieColour;
    [SerializeField] GameObject resetButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance == null) return;
        Debug.Log(this.name + "GameMabnager is: " + GameManager.Instance);
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnResetGame += GameManager_OnResetGame;
        GameManager.Instance.OnGameTie += GameManager_OnGameTie;
        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
        GameManager.Instance.OnResetGame -= GameManager_OnResetGame;
        GameManager.Instance.OnGameTie -= GameManager_OnGameTie;
    }

    private void GameManager_OnGameTie(object sender, System.EventArgs e)
    {
        resultText.color = tieColour;
        resultText.text = "It's a Tie!";
        Show();
    }

    private void GameManager_OnResetGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        //Player Won
        if (GameManager.Instance.GetLocalPlayerColour() == e.playerColour)
        {
            resultText.color = winColour;
            resultText.text = "YOU WIN!";
        }
        //Player Lost
        else
        {
            resultText.color = looseColour;
            resultText.text = "You Loose";
        }

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        if (!NetworkManager.Singleton.IsHost) resetButton.SetActive(false);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
