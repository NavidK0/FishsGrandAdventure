using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class NiceDayEvent : IGameEvent
{
    public string Description => "Have a Nice Day! :)";
    public Color Color => Color.green;
    public GameEventType GameEventType => GameEventType.NiceDay;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
    }

    public void OnFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.None);
    }

    public void Cleanup()
    {
    }
}