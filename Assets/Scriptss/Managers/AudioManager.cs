using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instace { get; private set; }

    [SerializeField] private GameObject placeSound;
    [SerializeField] private GameObject hoverSound;
    [SerializeField] private GameObject clickSound;

    private void Awake()
    {
        Instace = this;
       
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlaceSoundEffect += GameManager_OnPlaceSoundEffect;
    }

    private void GameManager_OnPlaceSoundEffect(object sender, System.EventArgs e)
    {
        GameObject sfxTransform = Instantiate(placeSound);
        Destroy(sfxTransform, 1f);
    }

    public void PlayHoverSound()
    {
        GameObject sfxTransform = Instantiate(hoverSound);
        Destroy(sfxTransform, 1f);
    }

    public void PlayClickSound()
    {
        GameObject sfxTransform = Instantiate(clickSound);
        Destroy(sfxTransform, 1f);
    }


}
