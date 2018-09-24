using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VCNetworkManager : NetworkManager
{
	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);

		VoiceChat.Networking.VoiceChatNetworkProxy.OnManagerStartClient(client);
	}

	public override void OnStopClient()
	{
		VoiceChat.Networking.VoiceChatNetworkProxy.OnManagerStopClient();
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
	}

	public override void OnStartServer()
	{
		base.OnStartServer ();

		VoiceChat.Networking.VoiceChatNetworkProxy.OnManagerStartServer();
	}

	public override void OnStopServer()
	{
		base.OnStopServer ();

		VoiceChat.Networking.VoiceChatNetworkProxy.OnManagerStopServer();
	}

	public override void OnClientConnect(NetworkConnection connection)
	{
		base.OnClientConnect(connection);
	}

    /// <summary>
    /// Spawns the player then links the voice proxy to him
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="playerControllerId"></param>
    /*public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        Debug.Log("OnServerAddPlayer");

        ///
        //// This code is the overwritten OnServerAddPlayer code from Unity's source: https://bitbucket.org/Unity-Technologies/networking/src/5.3/Runtime/NetworkManager.cs
        //// Modified to get the player's object to be able to add the proxy to him!
        ///
        if (playerPrefab == null)
        {
            if (LogFilter.logError) { Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object."); }
            return;
        }

        if (playerPrefab.GetComponent<NetworkIdentity>() == null)
        {
            if (LogFilter.logError) { Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab."); }
            return;
        }

        if (playerControllerId < conn.playerControllers.Count && conn.playerControllers[playerControllerId].IsValid && conn.playerControllers[playerControllerId].gameObject != null)
        {
            if (LogFilter.logError) { Debug.LogError("There is already a player at that playerControllerId for this connections."); }
            return;
        }

        GameObject player;
        Transform startPos = GetStartPosition();
        if (startPos != null)
        {
            player = (GameObject)Instantiate(playerPrefab, startPos.position, startPos.rotation);
        }
        else
        {
            player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        ///
        ////
        ///

        //Simple version for testing
        //var player = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        //NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);



        Debug.Log("ConnectionId: " + conn.connectionId);

        //get the voice proxy thingy that makes noise
        GameObject voiceProxy = VoiceChat.Networking.VoiceChatNetworkProxy.getProxy(conn.connectionId);

        //reset the proxy's audio source position, then parent it to the player's camera
        voiceProxy.transform.position = new Vector3(0, 0, 0);
        voiceProxy.transform.SetParent(player.GetComponent<Camera>().transform, false);
    }*/
}
