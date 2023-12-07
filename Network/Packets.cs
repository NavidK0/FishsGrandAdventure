using System;
using FishsGrandAdventure.Game;
using UnityEngine;

namespace FishsGrandAdventure.Network;

[Serializable]
public class Packet
{
}

[Serializable]
public class PacketGameEvent : Packet
{
    public GameEventType GameEventType;
}

[Serializable]
public class PacketPlayersBlazed : Packet
{
}

[Serializable]
public class PacketSetPlayerMoveSpeed : Packet
{
    public float Speed;
}

[Serializable]
public class PacketSpawnExplosion : Packet
{
    public Vector3 Position;
    public float KillRange = 2.4f;
    public float DamageRange = 5f;
}

[Serializable]
public class PacketStrikeLightning : Packet
{
    public Vector3 Position;
}

[Serializable]
public class PacketGlobalTimeSpeedMultiplier : Packet
{
    public float Multiplier;
}

[Serializable]
public class PacketDestroyEffects : Packet
{
}

[Serializable]
public class PacketResetPlayedSpeed : Packet
{
}