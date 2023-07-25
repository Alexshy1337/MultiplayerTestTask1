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
    [SerializeField] private GameObject outOfLobbyUICont;

    private GameObject retryBtnGO;
    private Action onSignedIn;

    private void Awake()
    {
        retryBtnGO = transform.Find("RetryAuthBtn").GameObject();

        onSignedIn = () => {
            SSTools.ShowMessage("Successful sign in!", SSTools.Position.top, SSTools.Time.twoSecond);
            UIVisibilityManager.Instance.Auth_OutOfLobbyTransition();
        };

        playerNameInput.onValidateInput = InputValidationUtils.onValidate;

        authBtn.onClick.AddListener(() => {
            LobbyManager.Instance.Authenticate(playerNameInput.text, onSignedIn);
            UIVisibilityManager.Instance.HideAuth();
        });

        retryBtnGO.GetComponent<Button>().onClick.AddListener(() => {
            UIVisibilityManager.Instance.RetryAuth();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnSignInFailed += (object o, EventArgs e) => {
            SSTools.ShowMessage("Sign in failed!", SSTools.Position.bottom, SSTools.Time.twoSecond);
            UIVisibilityManager.Instance.ShowAuthRetryBtn();
        };
    }

}
