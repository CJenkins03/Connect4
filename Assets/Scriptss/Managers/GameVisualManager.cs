using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.5f;
    [SerializeField] private Transform redPiece;
    [SerializeField] private Transform yellowPiece;
    [SerializeField] private Transform highlightPiece;
    [SerializeField] private Transform placeEffectBlue;
    [SerializeField] private Transform placeEffectRed;
    [SerializeField] private List<GameObject> spawnedObjectList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
        GameManager.Instance.OnHighlightPieces += GameManager_OnHighlightPieces;
        GameManager.Instance.OnResetGame += GameManager_OnResetGame;
        spawnedObjectList = new List<GameObject>();
    }

    public override void OnDestroy()
    {
        GameManager.Instance.OnClickedGridPosition -= GameManager_OnClickedGridPosition;
        GameManager.Instance.OnHighlightPieces -= GameManager_OnHighlightPieces;
        GameManager.Instance.OnResetGame -= GameManager_OnResetGame;
    }


    private void GameManager_OnResetGame(object sender, System.EventArgs e)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        foreach (GameObject gameObject in spawnedObjectList)
        {
            Destroy(gameObject);
        }
    }

    private void GameManager_OnHighlightPieces(object sender, GameManager.OnHighlightPiecesEventArgs e)
    {
        SpawnHighlightPiecesRpc(e.piecesList);
    }

    private void GameManager_OnClickedGridPosition(object sender, GameManager.OnClickedGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerColour);
    }



    //Spawn corresponding piece based on the recieved playerColour e.g. red or blue 
    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerColour playerColour)
    {
        Transform prefab = null;
        switch (playerColour)
        {
            case GameManager.PlayerColour.blue:
                prefab = yellowPiece;
                SpawnBlueEffectRpc(x, y);
                break;
            case GameManager.PlayerColour.red:
                prefab = redPiece;
                SpawnRedEffectRpc(x, y);
                break;
            default:
                break;
        }

        Transform spawnedTransform = Instantiate(prefab, GetGridWorldPos(x, y), Quaternion.identity);
        spawnedTransform.GetComponent<NetworkObject>().Spawn(true);

        //Add to list that will be cleared at the end of each round
        spawnedObjectList.Add(spawnedTransform.gameObject);     
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnBlueEffectRpc(int x, int y)
    {
        Transform placeEffectTransform = Instantiate(placeEffectBlue, GetGridWorldPos(x, y), Quaternion.identity);
        Destroy(placeEffectTransform.gameObject, .6f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnRedEffectRpc(int x, int y)
    {
        Transform placeEffectTransform = Instantiate(placeEffectRed, GetGridWorldPos(x, y), Quaternion.identity);
        Destroy(placeEffectTransform.gameObject, .6f);
    }



    //Spawn highlight pieces to clearly represent the 4 connecting winning pieces
    private void SpawnHighlightPiecesRpc(List<Vector2Int> highlightList)
    {
        foreach (Vector2Int gridPos in highlightList)
        {
            Transform spawnedTransform = Instantiate(highlightPiece, GetGridWorldPos(gridPos.x, gridPos.y), Quaternion.identity);
            spawnedTransform.GetComponent<NetworkObject>().Spawn(true);
            spawnedObjectList.Add(spawnedTransform.gameObject);
        }
    }


    private Vector2 GetGridWorldPos(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x, -GRID_SIZE + y);
    }
}
