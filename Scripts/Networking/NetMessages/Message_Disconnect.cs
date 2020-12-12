//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using Character;
using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/Disconnect")]
    public class Message_Disconnect : ANetMessage
    {
        public override void Server_ReceiveMessage(int connectionID, ByteStream msgData, LLServer server)
        {
            server.DisconnectUser(connectionID);
        }

        /// <summary>
        /// Tells the local client to remove the specified client by its connectionID
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="client"></param>
        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int connectionID = msgData.PopInt32();

            if (client.NetUsers.ContainsKey(connectionID))
            {
                NetCharacterController netCharacterController = client.clientsCharacter[connectionID];
                Destroy(netCharacterController.gameObject);
                client.clientsCharacter.Remove(connectionID);
                
                client.NetUsers.Remove(connectionID);
            }
        }
    }
}