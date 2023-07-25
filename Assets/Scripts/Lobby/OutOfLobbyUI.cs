using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OutOfLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createBtn;
    [SerializeField] private Button joinBtn;
    
    private GameObject retryBtnGO;

    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private TMP_InputField createInput;
    [SerializeField] private TextMeshProUGUI playerName;
    private void Awake()
    {
        retryBtnGO = transform.Find("RetryOutOfLobbyBtn").GameObject();
        retryBtnGO.GetComponent<Button>().onClick.AddListener(() => {
            UIVisibilityManager.Instance.RetryOutOfLobby();
        });

        createBtn.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(createInput.text, 4, false);
            UIVisibilityManager.Instance.HideOutOfLobby();
        });

        joinBtn.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobbyByCode(joinInput.text);
            UIVisibilityManager.Instance.HideOutOfLobby();
        });

        joinInput.onValidateInput = InputValidationUtils.onValidate;
        createInput.onValidateInput = InputValidationUtils.onValidate;

        //playerName.text = "Your name is: " + LobbyManager.Instance.getPlayerName; // some sort of onEnable required


    }

    private void Start()
    {

        LobbyManager.Instance.OnJoinedLobby += (object o, LobbyManager.LobbyEventArgs e) =>
        {
            SSTools.ShowMessage("Successfully joined a lobby!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            UIVisibilityManager.Instance.ShowInLobby();
        };

        LobbyManager.Instance.OnLobbyCreateFail += (object o, EventArgs e) =>
        {
            SSTools.ShowMessage("Failed to create a lobby!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            UIVisibilityManager.Instance.ShowAuthRetryBtn();
        };

        LobbyManager.Instance.OnJoinLobbyFail += (object o, EventArgs e) => {
            SSTools.ShowMessage("Failed to join a lobby!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            UIVisibilityManager.Instance.ShowAuthRetryBtn();
        };

    }
}
