using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class NiceDayEvent : BaseGameEvent
{
    public override string Name => "Nice Day";
    public override string Description => "Have a nice day! :)";
    public override Color Color => Color.green;
    public override GameEventType GameEventType => GameEventType.NiceDay;

    public override void OnPreFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.None);
    }
}