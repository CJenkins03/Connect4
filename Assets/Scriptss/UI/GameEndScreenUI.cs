using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using System.Collections;
using System;

public class GameEndScreenUI : MonoBehaviour
{
   
    [SerializeField] private TextMeshProUGUI endText;
    private bool shutDown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (shutDown)
        {
            Debug.Log(NetworkManager.Singleton.ShutdownInProgress);
            if (!NetworkManager.Singleton.ShutdownInProgress)
            {
                shutDown = false;
                StartCoroutine(LoadMainMenu());            
            }
        }
    }

    private void GameManager_OnGameEnd(object sender, GameManager.OnGameEndEventArgs e)
    {
        if (e.playerColour == GameManager.Instance.GetLocalPlayerColour()) endText.text = "YOU WIN!";
        else endText.text = "You Lose";
        gameObject.SetActive(true);
    }


    
    [Rpc(SendTo.Server)]
    public void ShutDownRpc()
    {
        CallShutDownRpc();     
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallShutDownRpc()
    {
        shutDown = true;
        NetworkManager.Singleton.Shutdown();     
    }
 
    private IEnumerator LoadMainMenu()
    {    
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameEnd -= GameManager_OnGameEnd;
    }

}

