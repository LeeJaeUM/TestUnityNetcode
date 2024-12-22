using UnityEngine;
using Unity.Netcode;

public class RPCTest : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        //Only send an RPC to the server on the client that
        //owns the NetworkObject that owns this NetworkBehaviour instance
        if (!IsServer && IsOwner) 
        {
            TestServerRpc(0, NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TestClientRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}"); 

        //Only send an RPC to the server on the client that
        //owns the NetworkObject that owns this NetworkBehaviour instance
        if (IsOwner) {
            TestServerRpc(value + 1, sourceNetworkObjectId);
        }
    }
    [Rpc(SendTo.Server)]
    void TestServerRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
        TestClientRpc(value, sourceNetworkObjectId);
    }
}
