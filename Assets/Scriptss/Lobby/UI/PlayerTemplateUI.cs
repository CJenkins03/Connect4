using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button kickPlayerButton;

    private Player player;

    public void SetKickPlayerButtonVisible(bool visible)
    {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player)
    {
        this.player = player;
        playerNameText.text = player.Data[LobbyScript.KEY_PLAYER_NAME].Value;
        //LobbyScript.PlayerCharacter playerCharacter =
        //    System.Enum.Parse<LobbyScript.PlayerCharacter>(player.Data[LobbyScript.KEY_PLAYER_CHARACTER].Value);
        //characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    }


    public void KickPlayer()
    {
        if (player != null)
        {
            LobbyScript.Instance.KickPlayer(player.Id);
        }

    }

    public void IconSelect()
    {
        LobbyScript.Instance.iconSelectUI.SetActive(true);
    }

}
