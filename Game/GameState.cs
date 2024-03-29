﻿using FishsGrandAdventure.Game.Events;
using JetBrains.Annotations;

namespace FishsGrandAdventure.Game;

public static class GameState
{
    [CanBeNull] public static BaseGameEvent CurrentGameEvent;

    public static GameEventType? ForceLoadEvent = GameEventType.None;
    public static float? ForcePlayerMovementSpeed = null;
}