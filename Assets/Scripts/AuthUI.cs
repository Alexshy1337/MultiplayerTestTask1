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
    private Button authBtn;
    private Button retryBtn;
    private TMP_InputField playerNameInput;
    [SerializeField] private string validCharacters = "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ-_0123456789";
    private GameObject outOfLobbyUICont;
    private GameObject authUICont;
    [SerializeField] private GameObject retryBtnGO;
    private Action onSignedIn;
    private Action onSignInFailed;

    private void Awake()
    {
        outOfLobbyUICont = transform.Find("outOfLobbyUIContainer").GameObject();
        authUICont = transform.Find("authUIContainer").GameObject();
        retryBtnGO = transform.Find("RetryBtn").GameObject();
        retryBtn = transform.Find("RetryBtn").GetComponent<Button>();
        authBtn = transform.Find("AuthBtn").GetComponent<Button>();
        playerNameInput = transform.Find("PlayerNameInput").GetComponent<TMP_InputField>();

        playerNameInput.onValidateInput = (string text, int charIndex, char addedChar) => {
            return ValidateChar(validCharacters, addedChar);
        };

        onSignInFailed = () => {
            SSTools.ShowMessage("Sign in failed!", SSTools.Position.top, SSTools.Time.twoSecond);
            retryBtnGO.SetActive(true);
        };

        onSignedIn = () => {
            SSTools.ShowMessage("Successful sign in!", SSTools.Position.top, SSTools.Time.twoSecond); 
            outOfLobbyUICont.SetActive(true);
        };

        authBtn.onClick.AddListener(() => {
            LobbyManager.Instance.Authenticate(playerNameInput.text, onSignedIn, onSignInFailed);
            authUICont.SetActive(false);
        });

        retryBtn.onClick.AddListener(() => {
            authUICont.SetActive(true);
            retryBtnGO.SetActive(false); 
        });
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }
}
