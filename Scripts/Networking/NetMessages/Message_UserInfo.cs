//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using GameSavvy.Byterizer;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/Messages/UserInfo")]
    public class Message_UserInfo : ANetMessage
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
        }

        public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
        {
            int connectionID = msgData.PopInt32();
            string userName = msgData.PopString();
            int teamNumber = msgData.PopInt32();

            var netUser = new NetUser
            {
                connectionID = connectionID,
                userName = userName,
                teamNumber = teamNumber
            };

            client.NetUsers[connectionID] = netUser;
        }
    }
}