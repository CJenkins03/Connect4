using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine.UI;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyScript : MonoBehaviour
{
    public static LobbyScript Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;


    private Lobby hostLobby;
    public Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    public string playerName;

    public Transform lobbyListContainer;
    public GameObject lobbyTemplateUI;

    public Player player;

    public GameObject setNameMenu;
    public GameObject mainMenu;
    public GameObject joinedLobbyUI;
    public GameObject iconSelectUI;



    public GameObject errorMessage;


    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

 
    public enum GameMode
    {
        FirstTo3,
        FirstTo5,
        FirstTo7
    }

    private void Awake()
    {
       Instance = this;
    }


  
    //Sends a request over the internet so it will take time
    //using await ensures the game does not freeze
    //Function will await a response from unity services before continuing and therefore the game will contiue to run 
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        //Events fires when a player successfully signs in/connects
        //AuthenticationService.Instance.SignedIn += () =>
        //{
        //    Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        //};

        //Will sign user in without any details required (e.g. username of password)
        //However this can be linked to accounts such as steam, apple, android ect.
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();    
        playerName = "CJ" + UnityEngine.Random.Range(1, 100);
        RefreshLobbyList();

        Debug.Log(joinedLobby);

    }



    float refreshTimer = 3f;

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();


        if (refreshTimer > 0)
        {
            refreshTimer -= Time.deltaTime;
        }
        else
        {
            RefreshLobbyList();
            refreshTimer = 3;
        }
       
    }

    public async void Authenticate()
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {
            // do nothing
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            RefreshLobbyList();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    //If there is no information sent to the lobby after 30 seconds the lobby will become inactive
    //An inactive lobby can not be joined or seen from outside players
    //This function sends a hearbeat to the lobby every 15 seconds to keep it active
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        //Only polls for updates if we are in a lobby
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                //Checks if player is still in the lobby, if not they have been kicked
                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                    KickedUI();
                    hostLobby = null;
                    joinedLobby = null;
                    return;
                }

                //Returns updated version of joined lobby
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                //Checks if game has started
                if (joinedLobby.Data["StartGame"].Value != "0")
                {
                    //Start game!
                    if (!IsLobbyHost())
                    {
                        RelayScript.Instace.JoinRelay(joinedLobby.Data["StartGame"].Value);
                    }
                    joinedLobby = null;
                }
            }
        }
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                    
                }
            }
        }
        return false;
    }

    public async void CreateLobby(bool isPrivate, string lobbyNameNew, GameMode gamemode)
    {
        //If lobby connection fails, entire game can break / casue errors
        //So we wrap this function in a try/catch
        //Catch-type lobbyServiceException to see what the error was
        try
        {
            player = GetPlayer();
            string lobbyName = lobbyNameNew;
            int maxPlayers = 2;
          

            //Lobby options are respoinsible for wether the lobby is public or private,
            //lobby passwords and more
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = isPrivate,

                Data = new Dictionary<string, DataObject>
                {
                    //Lobby host values
                    { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Member, gamemode.ToString()) },
                }

            };

            //Creates lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);


            //Assignes created lobby for easy access
            hostLobby = lobby;
            joinedLobby = hostLobby;
            //LobbyInformation.Instance.maxPlayers = joinedLobby.MaxPlayers;

            //Adds new lobby to the lobby browser and sets all required information
            Transform lobbyTemplate = Instantiate(lobbyTemplateUI.transform, lobbyListContainer);
            lobbyTemplate.GetComponent<LobbyTemplateUI>().SetLobbyValues(joinedLobby.Name, GetNumberOfLobbyPlayers(joinedLobby), joinedLobby.MaxPlayers, hostLobby);


            PrintPlayers(lobby);
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            GameModeManager.Instance.gameMode = gamemode;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            //Customise what type of lobbies appear and how they appear
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,

                //Only display lobbies that have more then 0 spaces
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },

                //In what order should the lobbies be displayed
                //First input: True = accending order, False = Decending order
                //Second input: in order of when lobby was created e.g. oldest to newest
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(); // Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbvies Found: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            LobbyJoinedUI.Instance.Show();

            Debug.Log("Joined lobby with code " + lobbyCode);
            PrintPlayers(joinedLobby);
         
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

            Debug.Log("quicked joined lobbyy");
        }
        catch (LobbyServiceException e)
        {
            errorMessage.SetActive(true);
            errorMessage.GetComponent<ErrorMessageUI>().errorMessage.text = "No Lobbies Found";
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            Player player = GetPlayer();

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            LobbyJoinedUI.Instance.Show();
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
            }
             
         };      
      
    }

    public void PrintPlayers()
    {
        if(joinedLobby != null) PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        //Debug.Log("Players in lobby " + "Lobby name: " + lobby.Name + " " + "GameMode: " + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

   
    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }


    public async void LeaveLobby()
    {
        try
        {
            Debug.Log("left lobby");
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            hostLobby = null;
            joinedLobbyUI.SetActive(false);
            mainMenu.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("kicking player: " + playerId);
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private void KickedUI()
    {
        joinedLobbyUI.SetActive(false);
        mainMenu.SetActive(true);
        errorMessage.SetActive(true);
        errorMessage.GetComponent<ErrorMessageUI>().errorMessage.text = "You Have Been Kicked";
    }


    public async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            }); 

            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            errorMessage.SetActive(true);
            errorMessage.GetComponent<ErrorMessageUI>().errorMessage.text = "Migrate Host Failed";
        }
    }


    private int GetNumberOfLobbyPlayers(Lobby lobby)
    {
        int numOfPlayers = 0;
        foreach (Player p in lobby.Players)
        {
            numOfPlayers++;
        }

        return numOfPlayers;
    }



    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync();


            UpdateLobbyList(lobbyListQueryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public void UpdateLobbyList(List<Lobby> lobbyList)
    {
        if (lobbyListContainer == null) return;
        foreach (Transform child in lobbyListContainer)
        {
            if (child == null) return;
            if (child == lobbyTemplateUI) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTemplateTransform = Instantiate(lobbyTemplateUI.transform, lobbyListContainer);
            lobbyTemplateTransform.GetComponent<LobbyTemplateUI>().SetLobbyValues(lobby.Name, 1, lobby.MaxPlayers, lobby);
        }
    }

    //public async void StartGame(string mapName)
    //{
    //    if (IsLobbyHost())
    //    {
    //        try
    //        {
    //            Debug.Log("Start Game");

    //            string relayCode = await RelayScript.Instace.CreateRelay();

    //            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
    //            {
    //                Data = new Dictionary<string, DataObject>
    //                {
    //                    { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
    //                }
    //            });

    //            joinedLobby = lobby;
    //            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

    //        }
    //        catch(LobbyServiceException e)
    //        {
    //            Debug.Log(e);
    //            errorMessage.SetActive(true);
    //            errorMessage.GetComponent<ErrorMessageUI>().errorMessage.text = "Start Game Failed";
    //        }
    //    }
    //}



    public async void StartGameForce()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Start Game");

                string relayCode = await RelayScript.Instace.CreateRelay();

                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });

                joinedLobby = lobby;
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);


            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                errorMessage.SetActive(true);
                errorMessage.GetComponent<ErrorMessageUI>().errorMessage.text = "Start Game Failed";
            }
        }
    }

    public void SetName(string name)
    {
        playerName = name;
        Authenticate();
        mainMenu.SetActive(true);
    }


    public void QuitGame()
    {
        Application.Quit();
    }



}


