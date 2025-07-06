using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyInformation : MonoBehaviour
{
    public static LobbyInformation Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public int maxPlayers = 0;


}
