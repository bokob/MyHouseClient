using System;
using Unity.Collections;
using Unity.Netcode;

[System.Serializable]
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes username;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId &&
            username == other.username;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref username);
    }
}
