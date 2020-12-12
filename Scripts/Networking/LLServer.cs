//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using GameSavvy.Byterizer;

namespace LLNet
{
    [System.Obsolete]
    public class LLServer : MonoBehaviour
    {
        [SerializeField] private int serverPort = 27000;
        [SerializeField] private int bufferSize = 1024;
        [SerializeField] private byte threadPoolSize = 3;

        [SerializeField] private NetMessageContainer netMessages;
        
        private byte reliableChannel;
        private byte unreliableChannel;
        private int socketId;

        private Dictionary<int, NetUser> netUsers;
        public Dictionary<int, NetUser> NetUsers => netUsers;

        public byte ReliableChannel => reliableChannel;

        private void Start()
        {
            StartServer();
        }

        private void StartServer()
        {
            netUsers = new Dictionary<int, NetUser>(16);
            GlobalConfig globalConfig = new GlobalConfig
            {
                ThreadPoolSize = threadPoolSize
            };

            NetworkTransport.Init(globalConfig);

            ConnectionConfig connectionConfig = new ConnectionConfig
            {
                SendDelay = 0,
                MinUpdateTimeout = 1
            };

            reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

            HostTopology hostTopology = new HostTopology(connectionConfig, 16);
            socketId = NetworkTransport.AddHost(hostTopology, serverPort);

            StartCoroutine(Receiver());

            Debug.Log($"StartServer => {socketId}");
        }

        private IEnumerator Receiver()
        {
            byte[] recBuffer = new byte[bufferSize];

            while (true)
            {
                NetworkEventType netEventType = NetworkTransport.Receive
                (
                    out int recSockId,
                    out int recConnectionId,
                    out int recChannelId,
                    recBuffer,
                    bufferSize,
                    out int recDataSize,
                    out byte error
                );

                if (error != 0)
                {
                    Debug.LogError($"Error ID => {error} :: {recConnectionId}");
                    // Server On User Disconnected error == 6 (timeout)
                    netUsers.Remove(recConnectionId);
                }
                else
                {
                    switch (netEventType)
                    {
                        case NetworkEventType.Nothing:
                            {
                                yield return null;
                                break;
                            }

                        case NetworkEventType.DataEvent:
                            {
                                OnDataReceived(recConnectionId, recChannelId, recBuffer, recDataSize);
                                break;
                            }

                        case NetworkEventType.ConnectEvent:
                            {
                                OnClientConnected(recConnectionId);
                                break;
                            }

                        case NetworkEventType.DisconnectEvent:
                            {
                                OnClientDisconnected(recConnectionId);
                                break;
                            }

                        default:
                            {
                                Debug.LogWarning($"@Server.Receiver.Default => Unrecognized Message Type");
                                break;
                            }
                    }
                }
            }
        }

        private void OnClientConnected(int recConnectionID)
        {
            if (netUsers.ContainsKey(recConnectionID))
            {
                Debug.Log($"@OnConnectedToServer => userId [{recConnectionID}] has Re-Connected");
            }
            else
            {
                var newUser = new NetUser
                {
                    connectionID = recConnectionID
                };
                
                netUsers[recConnectionID] = newUser;
                Debug.Log($"@Server.Receiver.Connect => userId [{recConnectionID}]");
            }
            
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.ConnectionAck);
            byteStream.Append(recConnectionID);
            
            SendNetMessage(recConnectionID, reliableChannel, byteStream.ToArray());
        }
        
        private void OnDataReceived(int clientId, int channel, byte[] data, int dataSize)
        {
            var msgData = new ByteStream(data, dataSize);
            var messageType = (NetMessageType)msgData.PopByte();

            if (netMessages.NetMessagesMap.ContainsKey(messageType))
            {
                netMessages.NetMessagesMap[messageType].Server_ReceiveMessage(clientId , msgData, this);
            }
        }

        private void OnClientDisconnected(int recConnectionId)
        {
            Debug.Log($"@Server.Receiver.Disconnect => connectionId[{recConnectionId}]");
        }

        public void SendNetMessage(int targetId, byte channel, byte[] data)
        {
            NetworkTransport.Send
            (
                socketId,
                targetId,
                channel,
                data,
                data.Length,
                out byte error
            );

            if (error != 0)
            {
                Debug.LogError($"@Server.SendNetMessage [{error}] : Could not send message to [{targetId}]");
            }
        }

        public void BroadcastNetMessage(byte channel, byte[] data, int? excludeId = null)
        {
            foreach (var user in netUsers)
            {
                if (excludeId != null && user.Key == excludeId) continue;

                NetworkTransport.Send
                (
                    socketId,
                    user.Key,
                    channel,
                    data,
                    data.Length,
                    out byte error
                );

                if (error != 0)
                {
                    Debug.LogError($"@Server.SendNetMessage [{error}] : Could not send message to [{user.Key}]");
                }
            }
        }

        public void MulticastNetMessage(int[] targets, byte channelId, byte[] data)
        {
            for (var i = 0; i < targets.Length; i++)
            {
                int connectionID = targets[i];
                SendNetMessage(connectionID, channelId, data);
            }
        }

        public void DisconnectUser(int connectionID)
        {
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.Disconnect);
            byteStream.Append(connectionID);

            NetUsers.Remove(connectionID);
            
            BroadcastNetMessage(ReliableChannel, byteStream.ToArray());
            NetworkTransport.Disconnect(socketId, connectionID, out byte error);
        }
        
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(16);
                    GUILayout.Label("<<< SERVER >>>");
                    GUILayout.Space(16);

                    foreach (var user in netUsers)
                    {
                        if (GUILayout.Button($"Kick  {user.Value.userName} : {user.Key}"))
                        {
                            DisconnectUser(user.Key);
                        }
                    }
                }
                
                GUILayout.EndVertical();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}