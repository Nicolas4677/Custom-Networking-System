//Copyright(C) 2020, Nicolas Morales Escobar. All rights reserved. 

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/ChatBroadcast")]
    public class Message_ChatBroadcast : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            var byteStream = new ByteStream();
            byteStream.Append((byte)NetMessageType.ChatBroadcast);

            string message = msgData.PopString();
            byteStream.Append(message);
            
            server.BroadcastNetMessage(server.ReliableChannel, byteStream.ToArray(), connectionID);
        }

        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            string userName = client.UserName;
            string message = msgData.PopString();
            
            client.AddMessageToQ($"{userName} >> {message}");
        }
    }
}