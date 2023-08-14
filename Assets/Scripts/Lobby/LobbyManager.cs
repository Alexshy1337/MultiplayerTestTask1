using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager Instance { get; private set; }
    
    public const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public const string KEY_PLAYER_NAME = "PlayerName";

    public event EventHandler OnLeftLobby;
    public event EventHandler OnSignInFailed;
    public event EventHandler OnJoinLobbyFail;
    public event EventHandler OnLobbyCreateFail;
    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    private bool lobbyPollEnabled = true;
    private float lobbyPollTimer;
    private float heartbeatTimer;
    private Lobby joinedLobby;
    private string playerName;
    public string getPlayerName { get { return playerName; } }
    public string getPlayerId { get { return AuthenticationService.Instance.PlayerId; } }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async void Authenticate(string playerName, Action OnSignedIn)
    {
        try
        {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);
        await UnityServices.InitializeAsync(initializationOptions);
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        SSTools.ShowMessage("Signing in...", SSTools.Position.bottom, SSTools.Time.twoSecond);

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); 
        }
        catch(Exception ex) {
            Debug.Log(ex);
            OnSignInFailed?.Invoke(this, EventArgs.Empty);
        };

    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 20f;
                heartbeatTimer = heartbeatTimerMax;

                //Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    /*
    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }
    */

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
        });
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)//, GameMode gameMode)
    {
        try
        {
            Player player = GetPlayer();

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            SSTools.ShowMessage("Created Lobby " + lobby.Name, SSTools.Position.top, SSTools.Time.twoSecond);





        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            OnLobbyCreateFail?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
            {
                Player = player
            });

            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch(Exception ex) { 
            Debug.Log(ex);
            OnJoinLobbyFail?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            Player player = GetPlayer();

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            OnJoinLobbyFail?.Invoke(this, EventArgs.Empty);
        }
    }

    private async void HandleLobbyPolling()
    {
        if (lobbyPollEnabled && joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.5f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value != "0" && !IsLobbyHost())
                {
                    lobbyPollEnabled = false;
                    Loader.Load(Loader.Scene.GameScene);
                }

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }



    //relay section


    


    public void StartGame()
    {


        //joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4,);

        /*
        Allocation allocation = await AllocateRelay();

        string relayJoinCode = await GetRelayJoinCode(allocation);

        await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                     { KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                 }
        });

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        */
        Loader.Load(Loader.Scene.GameScene);


    }

}

