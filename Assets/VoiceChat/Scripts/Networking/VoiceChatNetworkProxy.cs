using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace VoiceChat.Networking
{
    /// <summary>
    /// Effectively the voice chat audio source. This creates a voice chat player and makes the sounds emit from it. One of these attaches to every player
    /// </summary>
    public class VoiceChatNetworkProxy : NetworkBehaviour
    {
        public delegate void MessageHandler<T>(T data);
        public static event MessageHandler<VoiceChatPacketMessage> VoiceChatPacketReceived;

        VoiceChatPlayer player = null;
        
        void Start()
        {
            if (isLocalPlayer)
            {
                if (LogFilter.logDebug)
                {
                    Debug.Log("Setting VoiceChat recorder NetworkId.");
                }

                VoiceChatRecorder.Instance.NewSample += OnNewSample;
                VoiceChatRecorder.Instance.NetworkId = (int) GetComponent<NetworkIdentity>().netId.Value;
            }
            else
            {
                //links all clients into VoiceChatPacketReceived to send the audio to the right player
                VoiceChatPacketReceived += OnReceivePacket;
            }

            if (isClient && (!isLocalPlayer || VoiceChatSettings.Instance.LocalDebug))
            {
                gameObject.AddComponent<AudioSource>();
                player = gameObject.AddComponent<VoiceChatPlayer>();
				Debug.Log("Created voice chat player");
            }
        }

        void OnDestroy()
        {
            if (VoiceChatRecorder.Instance != null)
                VoiceChatRecorder.Instance.NewSample -= OnNewSample;
            VoiceChatPacketReceived -= OnReceivePacket;
        }

        /// <summary>
        /// Runs on proxies when they receive voice packets. Sends the message to the player if this is the correct proxy to play the sound
        /// </summary>
        /// <param name="data"></param>
        private void OnReceivePacket(VoiceChatPacketMessage data)
        {

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Received a new Voice Sample. Playing!");
            //}

            if (data.netId == GetComponent<NetworkIdentity>().netId.Value)
                player.OnNewSample(data.packet);
        }

        void OnNewSample(VoiceChatPacket packet)
        {
            var packetMessage = new VoiceChatPacketMessage {
                netId = (short) packet.NetworkId,
                packet = packet,
            };

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Got a new Voice Sample. Streaming!");
            //}

            NetworkManager.singleton.client.SendUnreliable(VoiceChatMsgType.Packet, packetMessage);
        }
      
        #region NetworkManager Hooks

        public static void OnManagerStartClient(NetworkClient client, GameObject customPrefab = null)
        {
            client.RegisterHandler(VoiceChatMsgType.Packet, OnClientPacketReceived);
        }

        public static void OnManagerStopClient()
        {
            var client = NetworkManager.singleton.client;
            if (client == null) return;

            client.UnregisterHandler(VoiceChatMsgType.Packet);
        }

        public static void OnManagerStartServer()
        {
            NetworkServer.RegisterHandler(VoiceChatMsgType.Packet, OnServerPacketReceived);
        }

        public static void OnManagerStopServer()
        {
            NetworkServer.UnregisterHandler(VoiceChatMsgType.Packet);
        }

        #endregion

        #region Network Message Handlers
        
        /// <summary>
        /// Runs when the server receives a voice transmission packet. Sends the packet to its players and clients
        /// </summary>
        /// <param name="netMsg"></param>
        private static void OnServerPacketReceived(NetworkMessage netMsg)
        {
            VoiceChatPacketMessage data = netMsg.ReadMessage<VoiceChatPacketMessage>();

            foreach (NetworkConnection connection in NetworkServer.connections)
            {
                if (connection == null || connection.playerControllers.Count <= 0 || connection.playerControllers[0].gameObject.GetComponent<NetworkIdentity>().netId.Value == data.netId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

            foreach (NetworkConnection connection in NetworkServer.localConnections)
            {
                if (connection == null || connection.playerControllers.Count <= 0 || connection.playerControllers[0].gameObject.GetComponent<NetworkIdentity>().netId.Value == data.netId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

        }
        
        /// <summary>
        /// Runs when a client receives a voice transmission packet. Sends it to the proper audio source (VoiceChatPacketReceived calls all clients' OnReceivePacket)!
        /// </summary>
        /// <param name="netMsg"></param>
        private static void OnClientPacketReceived(NetworkMessage netMsg)
        {
            if (VoiceChatPacketReceived != null)
            {
                VoiceChatPacketMessage data = netMsg.ReadMessage<VoiceChatPacketMessage>();
                VoiceChatPacketReceived(data);
            }
        }
        
        #endregion
    }
}