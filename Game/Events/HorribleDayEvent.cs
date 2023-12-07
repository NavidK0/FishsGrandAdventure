using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HorribleDayEvent : IGameEvent
{
    public string Description => "What a Horrible Night to Have a Curse";
    public Color Color => Color.red;
    public GameEventType GameEventType => GameEventType.HorribleDay;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
    }

    public void OnFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.Eclipsed);
    }

    public void Cleanup()
    {
    }
}