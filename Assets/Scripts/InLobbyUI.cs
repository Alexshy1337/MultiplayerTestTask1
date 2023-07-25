using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InLobbyUI : MonoBehaviour
{
    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Button startGameBtn;


    private void Awake()
    {
        playerSingleTemplate.gameObject.SetActive(false);
        //OnJoinedLobbyUpdate


        leaveLobbyBtn.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
        });

        startGameBtn.onClick.AddListener(() =>
        {
            SSTools.ShowMessage("Starting the game!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            //loadscene(
        });


    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;

        
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        UIVisibilityManager.Instance.InLobby_OutLobbyTransition();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();
        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            PlayerTemplateUI playerTemplateUI = playerSingleTransform.GetComponent<PlayerTemplateUI>();

            playerTemplateUI.UpdatePlayer(player);
        }


        lobbyNameText.text = "Joined " + lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        lobbyCodeText.text = "join code: " + lobby.LobbyCode;
        UIVisibilityManager.Instance.ShowInLobby();
    }

    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

}
