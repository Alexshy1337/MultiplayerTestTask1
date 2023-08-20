using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor.PackageManager;
using UnityEngine;

public class MPGameManager : NetworkBehaviour
{
    public class PlayerData
    {
        public UInt64 Id;
        public string Name;
        public short Health;
        public short CoinsCollected;
    }

    public static MPGameManager instance { get; private set; }
    [SerializeField] private List<PlayerData> _players;
    [SerializeField] private List<Transform> spawnPoints;

    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private Transform coinPrefab;  
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] public static readonly float BULLET_SPEED = 15f;
    [SerializeField] public static readonly short BULLLET_DAMAGE = 34;
    [SerializeField] public static readonly short PLAYER_HEALTH = 100;

    private void Awake()
    {
        instance = this;
        StartGame();
    }

    public override void OnNetworkSpawn()
    {
        RegisterPlayerServerRpc(LobbyManager.Instance.getPlayerName); // serverRpcParams will be filled in automatically
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += UnregisterMissingPlayers;
            SpawnCoins();
        }
    }

    

    //NetworkManager.SpawnManager

    //NetworkManager.OnClientDisconnectCallback

    //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;


    private void StartGame()
    {
        if (LobbyManager.Instance.IsLobbyHost())
        {
            _players = new List<PlayerData>();
            CreateRelay(LobbyManager.Instance.GetJoinedLobby());
        }
        else
        {
            JoinRelay(LobbyManager.Instance.GetJoinedLobby().Data[LobbyManager.KEY_RELAY_JOIN_CODE].Value);
        }

    }

    private async void CreateRelay(Lobby joinedLobby)
    {
        Allocation allocation = await AllocateRelay();
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        UpdateLobbyWithRelayCode(joinedLobby.Id, allocation);
        NetworkManager.Singleton.StartHost();
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
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void SpawnCoins()
    {
        for(int i =0; i < 10; i++)
        {
            var coin = Instantiate(coinPrefab, RandomVector(), Quaternion.identity);
            coin.GetComponent<NetworkObject>().Spawn();
        }


    }

    private Vector3 RandomVector()
    {
        return 10 * (new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f));
    }

    private int FindPlayer(UInt64 clientId)
    {
        int index = -1;
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Id == clientId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    private void UnregisterPlayer(ulong disconnectingClientId)
    {
        var index = FindPlayer(disconnectingClientId);
        if (index != -1)
            _players.RemoveAt(index);
        else
            Debug.LogError("client id not found  while trying to unregister a player");
    }

    private void UnregisterMissingPlayers(ulong disconnectingClientId)
    {
        if (IsHost)
            for (int i = 0; i < _players.Count; i++)
                if (!NetworkManager.ConnectedClients.ContainsKey(_players[i].Id))
                    _players.RemoveAt(i);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {

        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            _players.Add(new PlayerData() { Id = clientId, Health = PLAYER_HEALTH, CoinsCollected = 0, Name = name });
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(Vector3 position, Vector3 direction) // no security against cheaters
    {
        var bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.GetComponent<Rigidbody2D>().velocity = BULLET_SPEED * direction.normalized;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var index = FindPlayer(clientId);
        Debug.LogFormat("TakeDamageServerRpc - name = {0}", name);
        Debug.LogFormat("index =  {0}" , index);
        Debug.LogFormat("clientId =  {0}", clientId);
        if (index != -1)
        {
            _players[index].Health -= BULLLET_DAMAGE;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            UpdateHealthTextClientRpc(_players[index].Health, clientRpcParams);
        }
        else
            Debug.LogError("client id not found while trying to take damage");

    }

    [ClientRpc]
    public void UpdateHealthTextClientRpc(short health, ClientRpcParams clientRpcParams)
    {
        Debug.LogFormat("upd health before if, player name = {0}" , LobbyManager.Instance.getPlayerName);
        
        
        if (!IsOwner) return;
        Debug.LogFormat("clientRpcParams.Send.TargetClientIds[0] = {0}" , clientRpcParams.Send.TargetClientIds[0]);
        Debug.LogFormat("NetworkManager.Singleton.LocalClientId = {0}", NetworkManager.Singleton.LocalClientId);
        Debug.LogFormat("upd health after if");
        healthText.text = "Health: " + health.ToString();

    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectCoinServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var index = FindPlayer(clientId);
        if (index != -1)
            _players[index].CoinsCollected += 1;
        else
            Debug.LogError("client id not found while trying to collect a coin");
    }

    
}


/*
 

[ServerRpc(RequireOwnership = false)]
    public void ServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        
    } 

 */