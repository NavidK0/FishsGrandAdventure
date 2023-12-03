namespace FishsGrandAdventure.Game;

public static class GameState
{
    public static GameEvent CurrentEvent = GameEvent.None;

    public static SelectableLevel ForceLoadLevel = null;
    public static GameEvent? ForceLoadEvent = GameEvent.None;
    public static float? ForcePlayerMovementSpeed = null;
}