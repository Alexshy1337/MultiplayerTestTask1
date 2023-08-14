using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MPGameManager : NetworkBehaviour
{

    public static MPGameManager instance { get; private set; }

    private event EventHandler OnClientConnect;
    private event EventHandler OnClientDisconnect;
    private List<PlayerData> _players;

    private void Awake()
    {
        instance = this;
        StartGame();
    }


    //NetworkManager.SpawnManager

    //NetworkManager.OnClientDisconnectCallback

    //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;


    private void StartGame()
    {
        if (LobbyManager.Instance.IsLobbyHost())
        {
            CreateRelay(LobbyManager.Instance.GetJoinedLobby());
            NetworkManager.Singleton.StartHost();//returns bool, so should be used for actions on failure
        }
        else
        {
            JoinRelay(LobbyManager.Instance.GetJoinedLobby().Data[LobbyManager.KEY_RELAY_JOIN_CODE].Value);
            NetworkManager.Singleton.StartClient();//returns bool, so should be used for actions on failure
        }

    }

    public void RegisterPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (IsHost)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                _players.Add(new PlayerData() { Id = clientId, Health = 100, CoinsCollected = 0, Name = ""}) ;
            }
        }


        //SSTools.ShowMessage("lobby player id = " + LobbyManager.Instance.GetJoinedLobby(), SSTools.Position.bottom, SSTools.Time.twoSecond);
    }

    public override void OnNetworkSpawn()
    {
        RegisterPlayerServerRpc(); // serverRpcParams will be filled in automatically
    }

    private async void CreateRelay(Lobby joinedLobby)
    {
        Allocation allocation = await AllocateRelay();
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        UpdateLobbyWithRelayCode(joinedLobby.Id, allocation);
    }

    private async void UpdateLobbyWithRelayCode(string lobbyId, Allocation allocation)
    {
        string relayJoinCode = await GetRelayJoinCode(allocation);

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                     { LobbyManager.KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            }
        });
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public struct PlayerData
    {
        public UInt64 Id; // NetworkManager -> ClientConnectionId
        public string Name;
        public short Health;
        public short CoinsCollected;
    }
}
