using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class NiceDayEvent : BaseGameEvent
{
    public override string Description => "Have a Nice Day! :)";
    public override Color Color => Color.green;
    public override GameEventType GameEventType => GameEventType.NiceDay;

    public override void OnPreFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.None);
    }
}