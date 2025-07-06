using UnityEngine;

public class GridPosition : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] public int x;
    [SerializeField] public int y;
    [SerializeField] private GameObject boardPiece;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

  
    private void OnMouseDown()
    {
        Debug.Log(x + "_" + y);
        if(GameManager.Instance != null) GameManager.Instance.ClickedOnGridPosRpc(x, y, GameManager.Instance.GetLocalPlayerColour());
    }

    public void SetBoardPiece(GameObject PlacedboardPiece)
    {
        boardPiece = PlacedboardPiece;
    }

    public GameObject GetBoardPiece()
    {
        return boardPiece;
    }



}
