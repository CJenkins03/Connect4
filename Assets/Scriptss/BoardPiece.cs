using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{

    [SerializeField] private GameObject highlightVisual;
 
    public void ShowHighlightVisual()
    {
        highlightVisual.SetActive(true);
    }
}
