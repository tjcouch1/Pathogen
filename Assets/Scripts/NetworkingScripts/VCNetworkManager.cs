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
}
