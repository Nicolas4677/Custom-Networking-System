//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/ChatWhisper")]
    public class Message_ChatWhisper : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            int target = msgData.PopInt32();
            string message = msgData.PopString();
            
            ByteStream byteStream = new ByteStream();
            byteStream.Append((byte)NetMessageType.ChatWhisper);
            byteStream.Append(connectionID);
            byteStream.Append(message);
            
            server.SendNetMessage(target, server.ReliableChannel, byteStream.ToArray());
        }

        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int sender = msgData.PopInt32();
            string message = msgData.PopString();
            
            string msg = $"< <[{client.NetUsers[sender].userName}]< <  {message}";
            client.AddMessageToQ(msg);
        }
    }
}