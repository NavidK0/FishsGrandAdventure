using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HorribleDayEvent : BaseGameEvent
{
    public override string Name => "Horrible Night";
    public override string Description => "What a horrible day to have a curse...";
    public override Color Color => Color.red;
    public override GameEventType GameEventType => GameEventType.HorribleDay;

    public override void OnPreFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.Eclipsed);
    }
}