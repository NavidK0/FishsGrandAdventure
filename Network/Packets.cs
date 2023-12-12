using System;
using System.Collections.Generic;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Utils;
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
public class PacketGameTip : Packet
{
    public string Header;
    public string Body;
}

[Serializable]
public class PacketDestroyEffects : Packet
{
}

[Serializable]
public class PacketResetPlayerSpeed : Packet
{
}

[Serializable]
public class PacketSpawnEnemy : Packet
{
    public Type EnemyType;
    public int LevelId;
    public Vector3 Position;
    public ulong ClientId;
    public bool Force;
    public bool ForceOutside;
    public bool IsInside;
    public List<Type> ComponentsToAttach;
}

[Serializable]
public class PacketSpawnEnemyOutside : Packet
{
    public Type EnemyType;
    public int LevelId;
    public Vector3? Position;
    public bool ForceOutside;
    public List<Type> ComponentsToAttach;
}

[Serializable]
public class PacketSpawnEnemyInside : Packet
{
    public Type EnemyType;
    public int LevelId;
    public Vector3? Position;
    public bool ForceInside;
    public List<Type> ComponentsToAttach;
}

[Serializable]
public class PacketPlayMusic : Packet
{
    public string Name;
    public float Volume = 1f;
    public float Pitch = 1f;
    public bool Loop;
}

[Serializable]
public class PacketStopMusic : Packet
{
    public bool FadeOut;
    public float FadeOutDuration = 1f;
}

[Serializable]
public class PacketGrabItem : Packet
{
    public ulong ClientId;
    public ulong NetworkObjectId;
}