//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using System.Collections.Generic;
using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    
    [CreateAssetMenu(menuName = "LLNet/Messages/TeamMessage")]
    public class Message_TeamMessage : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            int teamNumber = msgData.PopInt32();
            string message = msgData.PopString();
            
            var targetsList =  new List<int>();

            foreach (var userPair in server.NetUsers)
            {
                NetUser netUser = userPair.Value;

                if (netUser.teamNumber == teamNumber && netUser.connectionID != connectionID)
                {
                    targetsList.Add(netUser.connectionID);
                }
            }

            int[] targets = targetsList.ToArray();
            
            var byteStream = new ByteStream();
            
            byteStream.Append((byte)NetMessageType.ChatTeamMessage);
            byteStream.Append(teamNumber);
            byteStream.Append(message);
            
            server.MulticastNetMessage(targets, server.ReliableChannel, byteStream.ToArray());
        }

        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int teamNumber = msgData.PopInt32();
            string message = msgData.PopString();
            client.AddMessageToQ($"Team_{teamNumber} >> {message}");
        }
    }
}