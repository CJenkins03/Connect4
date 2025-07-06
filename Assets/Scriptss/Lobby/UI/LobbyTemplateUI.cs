using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTemplateUI : MonoBehaviour
{
    private Lobby lobby;
    public TextMeshProUGUI lobbyName;
    public TextMeshProUGUI players;

    public void SetLobbyValues(string lobbyName, int players, int maxPlayers, Lobby lobby)
    {
        Debug.Log("setting lobby values");
        this.lobby = lobby;
        this.lobbyName.text = lobbyName;
        this.players.text = players + "/" + maxPlayers;
    }

    public void JoinLobby()
    {
        Debug.Log("Join lobby by press");
        LobbyScript.Instance.JoinLobby(lobby);
    }

    public void HoverSound()
    {
        AudioManager.Instace.PlayHoverSound();
    }

    public void ClickSound()
    {
        AudioManager.Instace.PlayClickSound();
    }
}
