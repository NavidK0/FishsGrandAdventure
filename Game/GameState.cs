using FishsGrandAdventure.Game.Events;

namespace FishsGrandAdventure.Game;

public static class GameState
{
    public static GameEventType CurrentGameEventType = GameEventType.None;
    public static IGameEvent CurrentGameEvent;

    public static GameEventType? ForceLoadEvent = GameEventType.None;
    public static float? ForcePlayerMovementSpeed = null;
}