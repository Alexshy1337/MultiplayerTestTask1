using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private Player player;

    public void UpdatePlayer(Player player)
    {
        this.player = player;
        playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;

    }

}
