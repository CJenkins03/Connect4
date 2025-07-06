using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridPositionsManager : MonoBehaviour
{

    [SerializeField] private List<GridPosition> gridPositionList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance == null) return;


        //AutoMatically sets all of the grid positions e.g 0_1, 0_2 ect..
        int gridIndex = 0;
        for (int x = 0; x < GameManager.Instance.boardArray.GetLength(0); x++)
        {
            for (int y = 0; y < GameManager.Instance.boardArray.GetLength(1); y++)
            {
                gridPositionList[gridIndex].x = x;
                gridPositionList[gridIndex].y = y;
                gridIndex++;
            }
        }

    }


 
}
