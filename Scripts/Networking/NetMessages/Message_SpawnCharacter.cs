//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/SpawnCharacter")]
    public class Message_SpawnCharacter : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            
        }

        /// <summary>
        /// Tells the local client to spawn a character for the connection ID sent form the server
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="client"></param>
        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int connectionID = msgData.PopInt32();
            
            client.SpawnClientCharacter(connectionID);
        }
    }
}