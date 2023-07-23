using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    [SerializeField] private Button authBtn;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TextMeshProUGUI playerName;

    [SerializeField] private GameObject outOfLobbyUICont;
    private GameObject authUICont;
    private GameObject retryBtnGO;

    private Action onSignedIn;

    private void Awake()
    {
        //outOfLobbyUICont = transform.Find("outOfLobbyUIContainer").GameObject();
        authUICont = transform.Find("authUIContainer").GameObject();
        retryBtnGO = transform.Find("RetryAuthBtn").GameObject();
        //authBtn = transform.Find("AuthBtn").GetComponent<Button>();
        //playerNameInput = transform.Find("PlayerNameInput").GetComponent<TMP_InputField>();

        onSignedIn = () => {
            SSTools.ShowMessage("Successful sign in!", SSTools.Position.top, SSTools.Time.twoSecond);
            outOfLobbyUICont.SetActive(true);
            playerName.text = "Your name is: " + LobbyManager.Instance.getPlayerName;
        };

        playerNameInput.onValidateInput = InputValidationUtils.onValidate;

        authBtn.onClick.AddListener(() => {
            LobbyManager.Instance.Authenticate(playerNameInput.text, onSignedIn);
            authUICont.SetActive(false);
        });

        retryBtnGO.GetComponent<Button>().onClick.AddListener(() => {
            authUICont.SetActive(true);
            retryBtnGO.SetActive(false); 
        });
    }

    private void Start()
    {
        retryBtnGO.SetActive(false);
        LobbyManager.Instance.OnSignInFailed += (object o, EventArgs e) => {
            SSTools.ShowMessage("Sign in failed!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            retryBtnGO.SetActive(true);
        };
    }

}
