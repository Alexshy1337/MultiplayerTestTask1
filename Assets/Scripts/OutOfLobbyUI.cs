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
    
    
    [SerializeField] private GameObject inLobbyUICont;
    private GameObject outOfLobbyUICont; 
    private GameObject retryBtnGO;

    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private TMP_InputField createInput;
    [SerializeField] private TextMeshProUGUI playerName;
    private void Awake()
    {
        outOfLobbyUICont = transform.Find("outOfLobbyUIContainer").GameObject();
        //inLobbyUICont = transform.Find("InLobbyUIContainer").GameObject();
        retryBtnGO = transform.Find("RetryOutOfLobbyBtn").GameObject();
        /*
        createBtn = outOfLobbyUICont.transform.Find("CreateBtn").GetComponent<Button>();
        joinBtn = outOfLobbyUICont.transform.Find("JoinBtn").GetComponent<Button>();
        joinInput = outOfLobbyUICont.transform.Find("JoinInput").GetComponent<TMP_InputField>();
        createInput = outOfLobbyUICont.transform.Find("CreateInput").GetComponent<TMP_InputField>();
        playerName = outOfLobbyUICont.transform.Find("LabelPName").GetComponent<TextMeshProUGUI>();
        */

        retryBtnGO.GetComponent<Button>().onClick.AddListener(() => {
            outOfLobbyUICont.SetActive(true);
            retryBtnGO.SetActive(false);
        });

        createBtn.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(createInput.text, 4, false);
            outOfLobbyUICont.SetActive(false);
        });

        joinBtn.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobbyByCode(joinInput.text);
            outOfLobbyUICont.SetActive(false);
        });

        joinInput.onValidateInput = InputValidationUtils.onValidate;
        createInput.onValidateInput = InputValidationUtils.onValidate;

        //playerName.text = "Your name is: " + LobbyManager.Instance.getPlayerName; // some sort of onEnable required


    }

    private void Start()
    {
        retryBtnGO.SetActive(false);
        outOfLobbyUICont.SetActive(false);

        LobbyManager.Instance.OnJoinedLobby += (object o, LobbyManager.LobbyEventArgs e) =>
        {
            SSTools.ShowMessage("Successfully joined a lobby!", SSTools.Position.top, SSTools.Time.twoSecond);
            inLobbyUICont.SetActive(true);
        };

        LobbyManager.Instance.OnLobbyCreateFail += (object o, EventArgs e) =>
        {
            SSTools.ShowMessage("Failed to create a lobby!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            retryBtnGO.SetActive(true);
        };

        LobbyManager.Instance.OnJoinLobbyFail += (object o, EventArgs e) => {
            SSTools.ShowMessage("Failed to join a lobby!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            retryBtnGO.SetActive(true);
        };

    }
}
