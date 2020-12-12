//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

namespace LLNet
{
    public enum NetMessageType : byte
    {
        ConnectionAck,
        UserInfo,
        ChatWhisper,
        ChatBroadcast,
        ChatTeamMessage,
        Disconnect,
        SpawnCharacter,
        CharacterMove
    }
}