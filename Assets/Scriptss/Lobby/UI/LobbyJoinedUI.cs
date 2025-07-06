using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyJoinedUI : MonoBehaviour
{
    public static LobbyJoinedUI Instance { get; private set; }

    public Transform playerListContainer;
    public GameObject playerTemplateUI;
    public GameObject visual;
    public TextMeshProUGUI lobbyCode;

    public GameObject startButton;




    public bool allPlayersConnected = false;
    public bool gameStart = false;
    public TextMeshProUGUI Timer;
    public float GameCountDownTimer;
    public TextMeshProUGUI lobbyInfoText;

    public TextMeshProUGUI playerCountText;

    public TextMeshProUGUI gameModeText;


    public GameObject menu;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        LobbyScript.Instance.OnJoinedLobby += Instance_OnJoinedLobby;
        LobbyScript.Instance.OnJoinedLobbyUpdate += Instance_OnJoinedLobbyUpdate;
        startButton.SetActive(false);
       
    }

    private void Instance_OnJoinedLobbyUpdate(object sender, LobbyScript.LobbyEventArgs e)
    {
        UpdateLobby(LobbyScript.Instance.GetJoinedLobby());
    }

    private void Instance_OnJoinedLobby(object sender, LobbyScript.LobbyEventArgs e)
    {
        UpdateLobby(LobbyScript.Instance.GetJoinedLobby());
        GameCountDownTimer = 30;
        Debug.Log("Joined lobby event called");
    }


    private void UpdateLobby(Lobby lobby)
    {
        //Sets the lobby code UI
        lobbyCode.text = LobbyScript.Instance.joinedLobby.LobbyCode;

        //Clears the connected player list
        ClearLobby();

        //Updates the player count
        playerCountText.text = lobby.Players.Count.ToString() + "/" + LobbyScript.Instance.GetJoinedLobby().MaxPlayers;
       // gameModeText.text = lobby.Data["GameMode"].Value.ToString();

        //Creates a new UI element for each connected player
        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerTemplateUI.transform, playerListContainer);
            playerSingleTransform.gameObject.SetActive(true);
            PlayerTemplateUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<PlayerTemplateUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyScript.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

      
        Show();

        //If current players are not equal to max players, continue to wait until max players is met
        if (!allPlayersConnected) WaitForPlayers();   
        if(allPlayersConnected && LobbyScript.Instance.IsLobbyHost()) startButton.SetActive(true);
    }


    public bool WaitForPlayers()
    {
        int players = 0;

        foreach (Player player in LobbyScript.Instance.GetJoinedLobby().Players)
        {
            players++;

        }

        if (players == LobbyScript.Instance.GetJoinedLobby().MaxPlayers)
        {      
            Debug.Log("sering counter to 30");
            GameCountDownTimer = 30;
            allPlayersConnected = true;
            gameStart = true;
            lobbyInfoText.text = "Waiting For Host...";
            return true;
        }

        return false;
    }

    private void ClearLobby()
    {
        if (playerListContainer != null)
        {
            foreach (Transform child in playerListContainer)
            {
                if (child == playerTemplateUI) continue;
                Destroy(child.gameObject);
            }
        }
    }


    public void Show()
    {
        if(visual != null) visual.SetActive(true);
        menu.SetActive(false);

    }

    public void Hide()
    {
        visual.SetActive(false);
    }


    public void PlayerLeft()
    {
        allPlayersConnected = false;
        gameStart = false;
        GameCountDownTimer = 30;
    }


    private bool CheckAllPlayersConnected()
    {
        int players = 0;

        foreach (Player player in LobbyScript.Instance.GetJoinedLobby().Players)
        {
            players++;

        }

        if (players == LobbyScript.Instance.GetJoinedLobby().MaxPlayers)
        {
            return true;
        }

        return false;
    }

    private void CheckAllPlayersAreStillConnected()
    {
        if (CheckAllPlayersConnected() == false)
        {
            allPlayersConnected = false;
            gameStart =false;
            GameCountDownTimer = 30;
            lobbyInfoText.gameObject.SetActive(true);
        }
    }
}
