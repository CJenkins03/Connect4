using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject menuVisual;

    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    private bool isPrivate;
    public string lobbyName;

    private LobbyScript.GameMode gameMode;


    public void SwitchGameMode()
    {
        switch (gameMode)
        {
            default:
            case LobbyScript.GameMode.FirstTo3:
                gameMode = LobbyScript.GameMode.FirstTo5;
                break;
            case LobbyScript.GameMode.FirstTo5:
                gameMode = LobbyScript.GameMode.FirstTo7;
                break;
            case LobbyScript.GameMode.FirstTo7:
                gameMode = LobbyScript.GameMode.FirstTo3;
                break;
        }
        UpdateText();
    }

    public void ChangePrivacy()
    {
        isPrivate = !isPrivate;
        UpdateText();
    }


    public void CreateLobby()
    {
        Debug.Log(string.IsNullOrWhiteSpace(lobbyName));
        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            lobbyName = "A" + Random.Range(1, 100) + Random.Range(1, 100) + Random.Range(1, 100);
        }
        LobbyScript.Instance.CreateLobby(isPrivate, lobbyName, gameMode);
        menuVisual.SetActive(false);
        visual.SetActive(false);
        LobbyJoinedUI.Instance.Show();
    }

    private void UpdateText()
    {
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        gameModeText.text = gameMode.ToString();
    }

    public void Show()
    {
        menuVisual.SetActive(false);
        visual.SetActive(true);
    }

    public void MenuButton()
    {
        menuVisual.SetActive(true);
        visual.SetActive(false);
    }

    public void SetLobbyName(string name)
    {
        lobbyName = name;
    }
}
