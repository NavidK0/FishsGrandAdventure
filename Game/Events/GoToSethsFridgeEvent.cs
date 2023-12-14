using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class GoToSethsFridgeEvent : BaseGameEvent
{
    public override string Description => "Free luxury vacation to Seth's Fridge! Enjoy your stay!";
    public override Color Color => Color.cyan;
    public override GameEventType GameEventType => GameEventType.GoToSethsFridge;

    public override void OnServerInitialize(SelectableLevel level)
    {
        int groupCredits = Object.FindObjectOfType<Terminal>().groupCredits;

        StartOfRound.Instance.currentLevel = CustomMoonManager.SethsFridgeLevel;
        StartOfRound.Instance.currentLevelID = CustomMoonManager.SethsFridgeLevel.levelID;
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

        StartOfRound.Instance.ChangeLevelClientRpc(CustomMoonManager.SethsFridgeLevel.levelID, groupCredits);
    }
}