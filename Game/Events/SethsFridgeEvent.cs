using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SethsFridgeEvent : IGameEvent
{
    public string Description => "Free luxury vacation to Seth's Fridge!";
    public Color Color => Color.cyan;
    public GameEventType GameEventType => GameEventType.SethsFridge;

    public void OnServerInitialize(SelectableLevel level)
    {
        int groupCredits = Object.FindObjectOfType<Terminal>().groupCredits;

        StartOfRound.Instance.currentLevel = CustomMoonManager.SethsFridgeLevel;
        StartOfRound.Instance.currentLevelID = CustomMoonManager.SethsFridgeLevel.levelID;
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

        StartOfRound.Instance.ChangeLevelClientRpc(CustomMoonManager.SethsFridgeLevel.levelID, groupCredits);
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        throw new System.NotImplementedException();
    }

    public void OnFinishGeneratingLevel()
    {
        throw new System.NotImplementedException();
    }

    public void Cleanup()
    {
        throw new System.NotImplementedException();
    }
}