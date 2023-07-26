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



    private void Awake()
    {
        instance= this;

        StartGame();
    }


    private async void StartGame()
    {
        if (LobbyManager.Instance.IsLobbyHost())
        {
            CreateRelay(LobbyManager.Instance.GetJoinedLobby());
            
        }
        else
        {
            JoinAllocation joinAllocation = await JoinRelay(LobbyManager.Instance.GetJoinedLobby().Data[LobbyManager.KEY_RELAY_JOIN_CODE].Value);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            NetworkManager.Singleton.StartClient();
        }

    }


    private async void CreateRelay(Lobby joinedLobby)
    {

        Allocation allocation = await AllocateRelay();

        string relayJoinCode = await GetRelayJoinCode(allocation);

        await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                     { LobbyManager.KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                 }
        });

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        NetworkManager.Singleton.StartHost();
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

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
}
