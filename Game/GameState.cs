namespace FishsGrandAdventure.Game;

public static class GameState
{
    public static bool ShouldForceLoadEvent = false;
    public static GameEvent ForceLoadEvent = GameEvent.None;

    public static GameEvent CurrentEvent = GameEvent.None;
}