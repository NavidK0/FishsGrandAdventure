using FishsGrandAdventure.Game.Events;

namespace FishsGrandAdventure.Game;

public static class GameState
{
    public static IGameEvent CurrentGameEvent;
    public static GameEventType CurrentGameEventType => CurrentGameEvent.GameEventType;

    public static GameEventType? ForceLoadEvent = GameEventType.None;
    public static float? ForcePlayerMovementSpeed = null;
}