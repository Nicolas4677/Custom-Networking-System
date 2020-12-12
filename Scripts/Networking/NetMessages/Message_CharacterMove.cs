//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/CharacterMove")]
    public class Message_CharacterMove : ANetMessage
    {
        /// <summary>
        /// Sends the information to all clients of a character that requires to be moved
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="msgData"></param>
        /// <param name="server"></param>
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            NetUser netUser = server.NetUsers[connectionID];
            netUser.targetPos = msgData.PopVector3();

            server.NetUsers[connectionID] = netUser;
            
            msgData.ResetBuffer();
            
            msgData.Append((byte)NetMessageType.CharacterMove);
            msgData.Append(connectionID);
            msgData.Append(netUser.targetPos);
            
            server.BroadcastNetMessage(server.ReliableChannel, msgData.ToArray(), connectionID);
        }

        /// <summary>
        /// Moves the character of the clients specified by its connectionID to the position sent by the server
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="client"></param>
        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int connectionID = msgData.PopInt32();
            Vector3 pos = msgData.PopVector3();
            
            client.clientsCharacter[connectionID].MoveTo(pos);
        }
    }
}