using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HorribleDayEvent : BaseGameEvent
{
    public override string Description => "What a Horrible Night to Have a Curse";
    public override Color Color => Color.red;
    public override GameEventType GameEventType => GameEventType.HorribleDay;

    public override void OnPreFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.Eclipsed);
    }
}