using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] private GameObject redText;
    [SerializeField] private GameObject yellowText;


    [SerializeField] private TextMeshProUGUI playerIconText;
    [SerializeField] private GameObject playerIconRed;
    [SerializeField] private GameObject playerIconBlue;
    [SerializeField] private SpriteRenderer blueCircle;
    [SerializeField] private SpriteRenderer redCircle;
    [SerializeField] private Color activeColour;
    [SerializeField] private Color unactiveColour;

    [SerializeField] private TextMeshProUGUI blueScoreText;
    [SerializeField] private TextMeshProUGUI redScoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
        yellowText.SetActive(false);
        redText.SetActive(false);   
        playerIconBlue.SetActive(false);
        playerIconRed.SetActive(false);
        playerIconText.gameObject.SetActive(false) ;


        GameManager.Instance.OnSwitchTurn += GameManager_OnSwitchTurn;
        GameManager.Instance.OnGameStart += GameManager_OnGameStart;
        GameManager.Instance.OnScoreChange += GameManager_OnScoreChange;
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnSwitchTurn -= GameManager_OnSwitchTurn;
        GameManager.Instance.OnGameStart -= GameManager_OnGameStart;
        GameManager.Instance.OnScoreChange -= GameManager_OnScoreChange;
    }

    private void GameManager_OnScoreChange(object sender, System.EventArgs e)
    {
        redScoreText.text = GameManager.Instance.redScore.Value.ToString("0");
        blueScoreText.text = GameManager.Instance.blueScore.Value.ToString("0");
    }

    private void GameManager_OnGameStart(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerColour() == GameManager.PlayerColour.red)
        {
            playerIconText.text = "You Are Red!";
            playerIconRed.SetActive(true);
        }
        else
        {
            playerIconText.text = "You Are Blue!";
            playerIconBlue.SetActive(true);
        }


        playerIconText.gameObject.SetActive(true);
        UpdateCurrentArrow();
    }

    private void GameManager_OnSwitchTurn(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayerColour() == GameManager.PlayerColour.red)
        {
            redCircle.color = activeColour;
            blueCircle.color = unactiveColour;
        }
        else
        {
            blueCircle.color = activeColour;
            redCircle.color = unactiveColour;
        }
    }


  
}
