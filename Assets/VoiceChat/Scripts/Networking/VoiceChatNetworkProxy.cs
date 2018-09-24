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
        public static event System.Action<VoiceChatNetworkProxy> ProxyStarted;

        [SyncVar]
        private int networkId;

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
                VoiceChatRecorder.Instance.NetworkId = networkId;
            }
            else
            {
                VoiceChatPacketReceived += OnReceivePacket;
            }

            if (isClient && (!isLocalPlayer || VoiceChatSettings.Instance.LocalDebug))
            {
                gameObject.AddComponent<AudioSource>();
                player = gameObject.AddComponent<VoiceChatPlayer>();
				Debug.Log("Created voice chat player");
            }

            if (ProxyStarted != null)
            {
                ProxyStarted(this);
            }
        }

        void OnDestroy()
        {
            if (VoiceChatRecorder.Instance != null)
                VoiceChatRecorder.Instance.NewSample -= OnNewSample;
            VoiceChatPacketReceived -= OnReceivePacket;
        }

        //TODO: Fix up for netId
        private void OnReceivePacket(VoiceChatPacketMessage data)
        {

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Received a new Voice Sample. Playing!");
            //}

            if (data.proxyId == networkId)
            {
                player.OnNewSample(data.packet);
            }
        }

        void OnNewSample(VoiceChatPacket packet)
        {
            var packetMessage = new VoiceChatPacketMessage {
                proxyId = (short)GetComponent<NetworkIdentity>().netId.Value,
                packet = packet,
            };

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Got a new Voice Sample. Streaming!");
            //}

            NetworkManager.singleton.client.SendUnreliable(VoiceChatMsgType.Packet, packetMessage);
        }



        void SetNetworkId(int networkId)
        {
            var netIdentity = GetComponent<NetworkIdentity>();
            if (netIdentity.isServer || netIdentity.isClient)
            {
                Debug.LogWarning("Can only set NetworkId before spawning");
                return;
            }

            this.networkId = networkId;
            //VoiceChatRecorder.Instance.NetworkId = networkId;
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

        //TODO: Fix this up for netId
        private static void OnServerPacketReceived(NetworkMessage netMsg)
        {
            VoiceChatPacketMessage data = netMsg.ReadMessage<VoiceChatPacketMessage>();

            foreach (var connection in NetworkServer.connections)
            {
                if (connection == null || connection.connectionId == data.netId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

            foreach (var connection in NetworkServer.localConnections)
            {
                if (connection == null || connection.connectionId == data.netId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

        }

        //TODO: Fix up the netMsg packet for netId
        private static void OnClientPacketReceived(NetworkMessage netMsg)
        {
            if (VoiceChatPacketReceived != null)
            {
                var data = netMsg.ReadMessage<VoiceChatPacketMessage>();
                VoiceChatPacketReceived(data);
            }
        }
        
        #endregion
    }
}