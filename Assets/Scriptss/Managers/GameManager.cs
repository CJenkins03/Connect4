using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{  
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public event EventHandler<OnHighlightPiecesEventArgs> OnHighlightPieces;
    public event EventHandler OnGameStart;
    public event EventHandler OnSwitchTurn;
    public event EventHandler OnResetGame;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChange;
    public event EventHandler OnPlaceSoundEffect;
    public event EventHandler<OnGameEndEventArgs> OnGameEnd;


    public NetworkVariable<PlayerColour> currentPlayerColour = new NetworkVariable<PlayerColour>();
    public NetworkVariable<int> redScore = new NetworkVariable<int>();
    public NetworkVariable<int> blueScore = new NetworkVariable<int>();


    public PlayerColour[,] boardArray;
    public PlayerColour localPlayerColour;
    int piecesPlaced = 0;
    private int winRequirement;

    public enum PlayerColour
    {
        none,
        blue,
        red
    }

    public class OnGameWinEventArgs
    {
        public PlayerColour playerColour;
    }

    public class OnGameEndEventArgs
    {
        public PlayerColour playerColour;
    }

    public class OnHighlightPiecesEventArgs
    {
        public List<Vector2Int> piecesList;
    }
  
    public class OnClickedGridPositionEventArgs
    {
        public int x;
        public int y;
        public PlayerColour playerColour;
    }


    private void Awake()
    {
        boardArray = new PlayerColour[7, 6];
        Instance = this;    
    }


    public override void OnNetworkSpawn()
    {
    
        SetPlayerTypes();
        SetWinRequirement();
        if (IsHost) NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;


        currentPlayerColour.OnValueChanged += (PlayerColour oldPlayerColour, PlayerColour newPlayerColour) =>
        {
            OnSwitchTurn?.Invoke(this, EventArgs.Empty);
        };

        redScore.OnValueChanged += (int oldRedScore, int newRedScore) =>
        {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
        };

        blueScore.OnValueChanged += (int oldBlueScore, int newBlueScore) =>
        {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
        };
    }

    private void SetWinRequirement()
    {
        switch (GameModeManager.Instance.gameMode)
        {
            case LobbyScript.GameMode.FirstTo3:
                winRequirement = 3;
                break;
            case LobbyScript.GameMode.FirstTo5:
                winRequirement = 5;
                break;
            case LobbyScript.GameMode.FirstTo7:
                winRequirement = 7;
                break;
            default:
                break;
        }
    }


    //Start Game once 2 clients have connected
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {    
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayerColour.Value = PlayerColour.blue;
            GameStartClientRpc();
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void GameStartClientRpc()
    {
        Debug.Log("Game start clientprc");
        OnGameStart?.Invoke(this, EventArgs.Empty);
  
    }


    //Radomises the hosts player type and sets the clients type to the opposite of the host
    private void SetPlayerTypes()
    {
        if (IsHost)
        {
            int ranNum = UnityEngine.Random.Range(1, 3);
            if (ranNum == 1) localPlayerColour = PlayerColour.blue;
            else localPlayerColour = PlayerColour.red;
        }
        else GetServerPlayerTypeRpc(NetworkManager.Singleton.LocalClientId);
    }


    //Returns the hosts playerType
    [Rpc(SendTo.Server)]
    public void GetServerPlayerTypeRpc(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        GetServerPlayerTypeClientRpc(localPlayerColour, clientRpcParams);

    }



    //Sets clients player type to opposite of hosts
    [ClientRpc]
    public void GetServerPlayerTypeClientRpc(PlayerColour hostPlayerType, ClientRpcParams clientRpcParams = default)
    {
        if (hostPlayerType == PlayerColour.blue) localPlayerColour = PlayerColour.red;
        else localPlayerColour = PlayerColour.blue;

    }




    [Rpc(SendTo.Server)]
    public void ClickedOnGridPosRpc(int x, int y, PlayerColour playerColour)
    {
        //Ensure only the player with the current playerColour can play
        if (playerColour != currentPlayerColour.Value) return;

        //Ensure a piece can not be played on a grid postion that isnt empty
        if (boardArray[x, y] != PlayerColour.none) return;

        //Cant place a peice if there is nothing below it
        if (y != 0)
        {
            if(boardArray[x, y - 1] == PlayerColour.none) return;
        }
           

        //Sets the given board pos to the given playerColour
        boardArray[x, y] = playerColour;
        piecesPlaced++;
        PlayPlaceSoundRpc();


        //Event that triggers serverRpc to spawn piece on the board
        OnClickedGridPosition?.Invoke(this, new OnClickedGridPositionEventArgs
        {
            x = x,
            y = y,
            playerColour = playerColour
        });


        //Check for win state
        CheckConnect4(x, y, playerColour);



        //Switch current turn
        switch (currentPlayerColour.Value)
        {
            case PlayerColour.blue:
                currentPlayerColour.Value = PlayerColour.red;
                break;
            case PlayerColour.red:
                currentPlayerColour.Value = PlayerColour.blue;
                break;
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayPlaceSoundRpc()
    {
        OnPlaceSoundEffect?.Invoke(this, EventArgs.Empty);
    }

    public PlayerColour GetLocalPlayerColour()
    {
        return localPlayerColour;
    }

    public PlayerColour GetCurrentPlayerColour()
    {
        return currentPlayerColour.Value;
    }



    private List<Vector2Int> highlightList = new List<Vector2Int>();
    private bool winner = false;

    //Check if there is 4 pieces in a line, based on the last piece placed
    public void CheckConnect4(int x, int y, PlayerColour playerColour)
    {
        CheckHorizontalMatches(x, y, playerColour);
        CheckVerticalMatches(x, y, playerColour);
        CheckDiagonalRightMatches(x, y, playerColour);
        CheckDiagonalLeftMatches(x, y, playerColour);


        //On the 42nd piece placed, if all spaces haven been taken up and there is no winner then it is a tie
        if (piecesPlaced >= 41)
        {
            bool tie = true;
            for (int xPos = 0; xPos < boardArray.GetLength(0); xPos++)
            {
                for (int yPos = 0; yPos < boardArray.GetLength(1); yPos++)
                {
                    if (boardArray[xPos, yPos] == PlayerColour.none) tie = false;
                }
            }
            if (tie) OnGameTieClientRpc();
        }
    }

    private void CheckHorizontalMatches(int x, int y, PlayerColour playerColour)
    {
        if (winner) return;
        int horizontalMatches = 1;
        highlightList.Clear();
        highlightList.Add(new Vector2Int(x, y));
        //HORIZONTAL
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (x + i > 6) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + i), y]) break;

            if (playerColour == boardArray[(x + i), y])
            {
                highlightList.Add(new Vector2Int(x + i, y));
                horizontalMatches++;
            }
        }
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (x + -i <= -1) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + -i), y]) break;

            if (playerColour == boardArray[(x + -i), y])
            {
                highlightList.Add(new Vector2Int(x - i, y));
                horizontalMatches++;
            }

        }
        if (horizontalMatches >= 4) HighlightWinningPieces(highlightList, playerColour);

    }

    private void CheckVerticalMatches(int x, int y, PlayerColour playerColour)
    {
        if (winner) return;
        int verticalMatches = 1;
        highlightList.Clear();
        highlightList.Add(new Vector2Int(x, y));

        //Iterates upwards from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + i >= 5) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[x, (y + i)]) break;

            if (playerColour == boardArray[x, (y + i)])
            {
                highlightList.Add(new Vector2Int(x, y + i));
                verticalMatches++;
            }
        }
        //Iterates downwards from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + -i <= -1) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[x, (y + -i)]) break;

            if (playerColour == boardArray[x, (y + -i)])
            {
                highlightList.Add(new Vector2Int(x, y + -i));
                verticalMatches++;
            }

        }

        if (verticalMatches >= 4) HighlightWinningPieces(highlightList, playerColour);
    }

    private void CheckDiagonalRightMatches(int x, int y, PlayerColour playerColour)
    {
        if (winner) return;
        int diagonalMatches = 1;
        highlightList.Clear();
        highlightList.Add(new Vector2Int(x, y));

        //Iterates upwards and to the right from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + i >= 5 || x + i >= 5) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + i), (y + i)]) break;

            if (playerColour == boardArray[(x + i), (y + i)])
            {
                highlightList.Add(new Vector2Int(x + i, y + i));
                diagonalMatches++;
            }

        }
        //Iterates downwards and to the left from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + -i <= -1 || x + -i <= -1) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + -i), (y + -i)]) break;

            if (playerColour == boardArray[(x + -i), (y + -i)])
            {
                highlightList.Add(new Vector2Int(x + -i, y + -i));
                diagonalMatches++;
            }
        }

        if (diagonalMatches >= 4) HighlightWinningPieces(highlightList, playerColour);
    }

    private void CheckDiagonalLeftMatches(int x, int y, PlayerColour playerColour)
    {
        if (winner) return;
        int diagonalMatches = 1;
        highlightList.Clear();
        highlightList.Add(new Vector2Int(x, y));

        //Iterates upwards and to the left from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + i >= 5 || x + -i >= -1) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + -i), (y + i)]) break;

            if (playerColour == boardArray[(x + -i), (y + i)])
            {
                highlightList.Add(new Vector2Int(x + -i, y + i));
                diagonalMatches++;
            }

        }
        //Iterates downwards and to the right from the current piece
        for (int i = 1; i < 5; i++)
        {
            //Break if outside of the board
            if (y + -i <= -1 || x + i >= 5) break;

            //Break if the colour next to us is different
            if (playerColour != boardArray[(x + i), (y + -i)]) break;

            if (playerColour == boardArray[(x + i), (y + -i)])
            {
                highlightList.Add(new Vector2Int(x + i, y + -i));
                diagonalMatches++;
            }
        }

        if (diagonalMatches >= 4) HighlightWinningPieces(highlightList, playerColour);
    }


    //If a winner is found then highlight the winning pieces (the winning pieces are the 4 pieces in a line)
    private void HighlightWinningPieces(List<Vector2Int> highlightPieces, PlayerColour playerColour)
    {          
        winner = true;
        OnHighlightPieces?.Invoke(this, new OnHighlightPiecesEventArgs
        {
            piecesList = highlightPieces
        });

        //Increase the winning players score
        if (playerColour == PlayerColour.red) redScore.Value++;
        else blueScore.Value++;

        if (blueScore.Value == winRequirement || redScore.Value == winRequirement)
        {
            OnGameEndClientRpc();
            currentPlayerColour.Value = PlayerColour.none;       
        }
        else
        {
            OnGameWinClientRpc();
            currentPlayerColour.Value = PlayerColour.none;
            StartCoroutine(ResetGameDelay());
        }
    }


    //Sends an rpc to all clients with the value of who won
    //This lets them either display and win or loose screen
    [Rpc(SendTo.ClientsAndHost)]
    private void OnGameWinClientRpc()
    {
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            playerColour = currentPlayerColour.Value
        });
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnGameEndClientRpc()
    {
        OnGameEnd?.Invoke(this, new OnGameEndEventArgs
        {
            playerColour = currentPlayerColour.Value
        }); ;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnGameTieClientRpc()
    {
        OnGameTie?.Invoke(this, EventArgs.Empty);
       
    }

    private IEnumerator ResetGameDelay()
    {
        yield return new WaitForSeconds(3);
        ResetGameRpc();
    }


    [Rpc(SendTo.Server)]
    public void ResetGameRpc()
    {
        //Reset all boards spaces to none
        for (int x = 0; x < boardArray.GetLength(0); x++)
        {
            for (int y = 0; y < boardArray.GetLength(1); y++)
            {
                boardArray[x, y] = PlayerColour.none;
            }
        }

        piecesPlaced = 0;
        winner = false;       
        currentPlayerColour.Value = PlayerColour.blue;
        ResetGameClientRpc();
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void ResetGameClientRpc()
    {
        OnResetGame?.Invoke(this, EventArgs.Empty);
    }



    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
    }


}
