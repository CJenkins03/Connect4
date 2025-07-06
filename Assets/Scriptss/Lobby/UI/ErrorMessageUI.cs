using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessageUI : MonoBehaviour
{
    public TextMeshProUGUI errorMessage;


    public void CloseError()
    {
        this.gameObject.SetActive(false);
    }
}
