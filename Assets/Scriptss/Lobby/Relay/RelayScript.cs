using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayScript : MonoBehaviour
{
    public static RelayScript Instace {get; private set;}

    private void Awake()
    {
        Instace = this;
        //DontDestroyOnLoad(this);
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //RelayServerData relayServerData = new AllocationUtils.ToRelayServerData() .//ToRelayServerData(allocation, "dtls");  //new RelayServerData(allocation, "dtls"); 
            //var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData (AllocationUtils.ToRelayServerData(allocation, "dtls"));
              
            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
           Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            //RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls"); 

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));


            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    
}
