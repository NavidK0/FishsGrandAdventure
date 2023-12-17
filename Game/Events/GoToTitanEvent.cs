using System.Linq;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class GoToTitanEvent : BaseGameEvent
{
    public override string Name => "Go To Titan";
    public override string Description => "Let's go to Titan! Why the hell not?";
    public override Color Color => Color.cyan;
    public override GameEventType GameEventType => GameEventType.GoToTitan;

    public override void OnServerInitialize(SelectableLevel level)
    {
        int groupCredits = Object.FindObjectOfType<Terminal>().groupCredits;

        SelectableLevel titanLevel =
            StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains("Titan"));

        if (titanLevel == null)
        {
            Plugin.Log.LogError("Could not find Titan level!");
            return;
        }

        StartOfRound.Instance.currentLevel = titanLevel;
        StartOfRound.Instance.currentLevelID = titanLevel.levelID;
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

        StartOfRound.Instance.ChangeLevelClientRpc(titanLevel.levelID, groupCredits);
    }
}