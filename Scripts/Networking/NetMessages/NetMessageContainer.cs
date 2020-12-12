//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/NetMessageContainer")]
    public class NetMessageContainer : ScriptableObject
    {
        [SerializeField] private ANetMessage[] netMessages;

        private Dictionary<NetMessageType, ANetMessage> netMessagesMap;
        public Dictionary<NetMessageType, ANetMessage> NetMessagesMap => netMessagesMap;

        private void OnEnable()
        {
            MapMessages();
        }

        private void MapMessages()
        {
            netMessagesMap = new Dictionary<NetMessageType, ANetMessage>(netMessages.Length);
            
            foreach (ANetMessage item in netMessages)
            {
                if (item == null || netMessagesMap.ContainsKey(item.MessageType))
                {
                    Debug.LogWarning($"Cannot Add Duplicate or NULL Message [{item}]", item);
                }
                else
                {
                    netMessagesMap[item.MessageType] = item;
                }
            }
            
            Debug.Log($"Mapping Done -> Added [{netMessagesMap.Count}] messages!");
        }
    }
}