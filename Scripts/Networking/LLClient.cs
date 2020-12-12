//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Character;
using GameSavvy.Byterizer;

namespace LLNet
{
    [System.Obsolete]
    public class LLClient : MonoBehaviour
    {
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 27000;
        [SerializeField] private int bufferSize = 1024;
        [SerializeField] private byte threadPoolSize = 3;
        
        [Header("User Data")]
        [SerializeField] private string userName = "Dude";
        [SerializeField] private int teamNumber = 0;

        [SerializeField] private NetMessageContainer netMessages;

        [SerializeField] private GameObject characterPrefab;

        [HideInInspector] public NetCharacterController myCharacterController;
        public Dictionary<int, NetCharacterController> clientsCharacter = new Dictionary<int, NetCharacterController>(); 

        private byte reliableChannel;
        private byte unreliableChannel;
        private int socketId = 0;
        private int serverConnectionId = 0;

        public int myConnectionID = -1;
        private Dictionary<int, NetUser> netUsers;

        #region Encapsulation
        
        public string UserName => userName;
        public int TeamNumber => teamNumber;
        public byte ReliableChannel => reliableChannel;
        public byte UnreliableChannel => unreliableChannel;
        public Dictionary<int, NetUser> NetUsers => netUsers;

        #endregion

        private void Start()
        {
            ConnectToServer();
        }

        private void ConnectToServer()
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
            socketId = NetworkTransport.AddHost(hostTopology, 0);

            serverConnectionId = NetworkTransport.Connect(socketId, serverAddress, serverPort, 0, 
                                                                out byte error);

            if (error != 0)
            {
                Debug.LogError($"Error Connecting to Server => [{error}]");
            }
            else
            {
                StartCoroutine(Receiver());
                Debug.Log($"ConnectedToServer => {socketId}");
            }
        }

        private void Update()
        {
            if (myCharacterController != null)
            {
                myCharacterController.ReadInput();
            }
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
                    Debug.LogError($"Error ID => {error}");
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
                                OnDataReceived(recChannelId, recBuffer, recDataSize);
                                break;
                            }

                        case NetworkEventType.ConnectEvent:
                            {
                                Debug.Log($"@Client.Receiver.Connect => Socket[{recSockId}], connectionId[{recConnectionId}]");
                                
                                OnConnectedToServer(recConnectionId);
                                AddMessageToQ($"< Server < You're connected.");
                                break;
                            }

                        case NetworkEventType.DisconnectEvent:
                            {
                                Debug.Log($"@Client.Receiver.Disconnect => Socket[{recSockId}], connectionId[{recConnectionId}]");
                                
                                OnDisconnectedFromServer(recConnectionId);
                                netUsers.Clear();
                                AddMessageToQ($"< Server < You've been disconnected.");
                                break;
                            }

                        default:
                            {
                                Debug.LogWarning($"@Client.Receiver.Default => Unrecognized Message Type");
                                break;
                            }
                    }
                }
            }
        }
        
        private void OnConnectedToServer(int recConnectionID)
        {
            if (netUsers.ContainsKey(recConnectionID))
            {
                Debug.Log($"@OnConnectedToServer => userId [{recConnectionID}] has Re-Connected");
            }
            else
            {
                var newUser = new NetUser
                {
                    connectionID = recConnectionID,
                    teamNumber = teamNumber,
                    userName = userName
                };

                myConnectionID = recConnectionID;
                netUsers[recConnectionID] = newUser;
                
                SpawnMyCharacter();

                Debug.Log($"@Server.Receiver.Connect => userId [{recConnectionID}]");
            }
            
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.ConnectionAck);
            byteStream.Append(netUsers[recConnectionID].userName);
            byteStream.Append(netUsers[recConnectionID].teamNumber);
            
            SendNetMessage(reliableChannel, byteStream.ToArray());
        }
        
        private void OnDataReceived(int channel, byte[] data, int dataSize)
        {
            var msgData = new ByteStream(data, dataSize);
            var messageType = (NetMessageType)msgData.PopByte();

            if (netMessages.NetMessagesMap.ContainsKey(messageType))
            {
                netMessages.NetMessagesMap[messageType].Client_ReceiveMessage(msgData, this);
            }
        }
        
        private void OnDisconnectedFromServer(int recConnectionId)
        {
            Debug.Log($"@Server.Receiver.Disconnect => connectionId[{recConnectionId}]");
            
            NetUser disconnectedUser = netUsers[recConnectionId];
            netUsers.Remove(recConnectionId);

            if (myCharacterController != null)
            {
                myCharacterController.onPositionSelected -= OnCharacterMove;
            }
        }

        public void SendNetMessage(byte channel, byte[] data)
        {
            NetworkTransport.Send
            (
                socketId,
                serverConnectionId,
                channel,
                data,
                data.Length,
                out var error
            );

            if (error != 0)
            {
                Debug.LogError($"@Server.SendNetMessage [{error}] : Could not send message to Server");
            }
        }

        [SerializeField] private string msgToSend;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(8);
                    GUILayout.Label($"__ [{teamNumber}] {userName} __ : {myConnectionID}");
                    GUILayout.Space(32);
                    msgToSend = GUILayout.TextField(msgToSend);
                    GUILayout.Space(16);

                    if (GUILayout.Button("Disconnect"))
                    {
                        Application.Quit();
                    }

                    if (GUILayout.Button("BROADCAST"))
                    {
                        var byteStream = new ByteStream();
                        byteStream.Append((byte)NetMessageType.ChatBroadcast);
                        byteStream.Append(msgToSend);
                        
                        SendNetMessage(reliableChannel, byteStream.ToArray());
                        
                        AddMessageToQ($"BC > {msgToSend}");
                        msgToSend = "";
                    }
                    
                    if (GUILayout.Button("TEAM MESSAGE"))
                    {
                        var byteStream = new ByteStream();
                        
                        byteStream.Append((byte)NetMessageType.ChatTeamMessage);
                        byteStream.Append(teamNumber);
                        byteStream.Append(msgToSend);
                        
                        SendNetMessage(reliableChannel, byteStream.ToArray());
                        
                        AddMessageToQ($"TM > {msgToSend}");
                        msgToSend = "";
                    }

                    GUILayout.Space(16);
                    foreach (var user in netUsers)
                    {
                        if (user.Value.connectionID == myConnectionID) continue;

                        if (GUILayout.Button($"[{user.Value.teamNumber}] {user.Value.userName} : {user.Key}"))
                        {
                            var byteStream = new ByteStream();
                            byteStream.Append((byte)NetMessageType.ChatWhisper);
                            byteStream.Append(user.Key);
                            byteStream.Append(msgToSend);
                            
                            SendNetMessage(reliableChannel, byteStream.ToArray());
                            
                            AddMessageToQ($">>> {msgToSend}");
                            msgToSend = "";
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(40);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Chat Messages: ");
                    GUILayout.Space(32);
                    foreach (var msg in chatMessages)
                    {
                        GUILayout.Label(msg);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private readonly Queue<string> chatMessages = new Queue<string>(16);
        
        public void AddMessageToQ(string msg)
        {
            chatMessages.Enqueue(msg);
            if (chatMessages.Count > 16)
            {
                chatMessages.Dequeue();
            }
        }

        private void OnApplicationQuit()
        {
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.Disconnect);
            
            SendNetMessage(reliableChannel, byteStream.ToArray());
        }

        private void SpawnMyCharacter()
        {
            myCharacterController = SpawnCharacter(Vector3.zero, Quaternion.identity);
            
            myCharacterController.onPositionSelected += OnCharacterMove;
        }

        /// <summary>
        /// Called whenever the local client clicks on the ground of the level
        /// </summary>
        /// <param name="targetPos"></param>
        private void OnCharacterMove(Vector3 targetPos)
        {
            netUsers[myConnectionID].targetPos = targetPos;
            
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.CharacterMove);
            byteStream.Append(targetPos);
            
            SendNetMessage(reliableChannel, byteStream.ToArray());
        }

        /// <summary>
        /// Spawns a character for a specific Client based on its connectionID
        /// </summary>
        /// <param name="connectionID"></param>
        public void SpawnClientCharacter(int connectionID)
        {
            Transform myTransform = myCharacterController.transform;
            NetCharacterController clientCharacterController = SpawnCharacter(myTransform.position, myTransform.rotation);

            clientsCharacter[connectionID] = clientCharacterController;
        }

        /// <summary>
        /// Spawns the character and return its NetCharacterController component
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public NetCharacterController SpawnCharacter(Vector3 pos, Quaternion rot)
        {
            GameObject characterGO = Instantiate(characterPrefab, pos, rot);

            return characterGO.GetComponent<NetCharacterController>();
        }
    }
}