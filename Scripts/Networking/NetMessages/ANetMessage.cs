//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [System.Serializable]
    public abstract class ANetMessage : ScriptableObject
    {
        [SerializeField] protected NetMessageType messageType;
        public NetMessageType MessageType => messageType;

        public abstract void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server);

        public abstract void Client_ReceiveMessage(ByteStream msgData, LLClient client);
    }
}