//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/ConnectionAck")]
    public class Message_ConnectionAck : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            string userName = msgData.PopString();
            int teamNumber = msgData.PopInt32();

            var netUser = new NetUser
            {
                connectionID = connectionID,
                userName = userName,
                teamNumber = teamNumber
            };

            server.NetUsers[connectionID] = netUser;
            
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.UserInfo);
            byteStream.Append(connectionID);
            byteStream.Append(userName);
            byteStream.Append(teamNumber);
            
            server.BroadcastNetMessage(server.ReliableChannel, byteStream.ToArray(), connectionID);
            
            byteStream.ResetBuffer();
            byteStream.Append((byte)NetMessageType.SpawnCharacter);
            byteStream.Append(connectionID);
            
            server.BroadcastNetMessage(server.ReliableChannel, byteStream.ToArray(), connectionID);
            

            foreach (var serverNetUser in server.NetUsers)
            {
                NetUser other = serverNetUser.Value;
                
                byteStream.ResetBuffer();
                
                byteStream.Append((byte)NetMessageType.UserInfo);
                byteStream.Append(other.connectionID);
                byteStream.Append(other.userName);
                byteStream.Append(other.teamNumber);
                
                server.SendNetMessage(connectionID, server.ReliableChannel, byteStream.ToArray());
                
                if(other.connectionID == connectionID) continue;
                
                byteStream.ResetBuffer();
                byteStream.Append((byte)NetMessageType.SpawnCharacter);
                byteStream.Append(other.connectionID);
            
                server.SendNetMessage(connectionID, server.ReliableChannel, byteStream.ToArray());
            }
        }
        
        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int connectionID = msgData.PopInt32();

            NetUser netUser = client.NetUsers[client.myConnectionID];

            netUser.connectionID = connectionID;

            client.NetUsers[client.myConnectionID] = netUser;
            client.myConnectionID = connectionID;
        }
    }
}