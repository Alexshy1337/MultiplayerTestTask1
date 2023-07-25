using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIVisibilityManager : MonoBehaviour
{
    [SerializeField] private GameObject authUICont;
    [SerializeField] private GameObject outOfLobbyUICont;
    [SerializeField] private GameObject inLobbyUICont;
    [SerializeField] private GameObject authRetryBtn;
    [SerializeField] private GameObject outOfLobbyRetryBtn;


    public static UIVisibilityManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        authUICont?.SetActive(true);
        outOfLobbyUICont?.SetActive(false);
        inLobbyUICont?.SetActive(false);
        authRetryBtn?.SetActive(false);
        outOfLobbyRetryBtn?.SetActive(false);


    }

    public void HideAuth()
    {
        authUICont?.SetActive(false);
    }

    public void ShowAuth()
    {
        authUICont?.SetActive(true);
    }

    public void HideOutOfLobby()
    {
        outOfLobbyUICont?.SetActive(false);
    }

    public void ShowOutOfLobby()
    {
        outOfLobbyUICont?.SetActive(true);
    }
    public void HideInLobby()
    {
        inLobbyUICont?.SetActive(false);
    }
    public void ShowInLobby()
    {
        inLobbyUICont?.SetActive(true);
    }
    public void Auth_OutOfLobbyTransition()
    {
        authUICont?.SetActive(false);
        outOfLobbyUICont?.SetActive(true);
    }

    public void OutOfLobby_InLobbyTransition() 
    {
        outOfLobbyUICont?.SetActive(false);
        inLobbyUICont?.SetActive(true);
    }

    public void InLobby_OutLobbyTransition()
    {
        inLobbyUICont?.SetActive(false);
        outOfLobbyUICont?.SetActive(true);
    }

    public void ShowAuthRetryBtn() 
    {
        authRetryBtn?.SetActive(true);
    }

    public void HideAuthRetryBtn()
    {
        authRetryBtn?.SetActive(false);
    }

    public void ShowOutOfLobbyRetryBtn()
    {
        outOfLobbyRetryBtn?.SetActive(true);
    }

    public void HideOutOfLobbyRetryBtn()
    {
        outOfLobbyRetryBtn?.SetActive(false);
    }

    public void RetryAuth()
    {
        HideAuthRetryBtn();
        ShowAuth();
    }

    public void RetryOutOfLobby()
    {
        HideOutOfLobbyRetryBtn();
        ShowOutOfLobby();
    }
}
