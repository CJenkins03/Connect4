using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    
    public LobbyScript.GameMode gameMode;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
 
}
