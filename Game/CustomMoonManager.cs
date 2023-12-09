using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game;

public static class CustomMoonManager
{
    public static int BaseLevelId;

    public static SelectableLevel SethsFridgeLevel;

    public static bool Loaded;

    [UsedImplicitly]
    public static readonly List<SelectableLevel> OriginalLevels = new List<SelectableLevel>();

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void AddCustomMoonsAwake(StartOfRound __instance)
    {
        if (Loaded) return;

        foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
        {
            OriginalLevels.Add(Object.Instantiate(selectableLevel));
        }

        BaseLevelId = StartOfRound.Instance.levels.Length - 1;

        SelectableLevel rendLevel =
            StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains("Rend"));
        SelectableLevel titanLevel =
            StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains("Titan"));

        SelectableLevel sethsFridgeLevel = AddMoonSethsFridge(rendLevel, titanLevel);
        SethsFridgeLevel = sethsFridgeLevel;
        StartOfRound.Instance.levels = StartOfRound.Instance.levels.AddToArray(SethsFridgeLevel);

        Plugin.Log.LogInfo(
            $"Added custom moon {SethsFridgeLevel.PlanetName} with level ID {SethsFridgeLevel.levelID}"
        );

        AddCustomMoonToTerminal(
            SethsFridgeLevel,
            1700,
            "sethsfridge",
            "You (probably) won't regret it!\n",
            "Please enjoy your stay!"
        );
    }

    [HarmonyPatch(typeof(StartOfRound), "OnDestroy")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void AddCustomMoonsOnDestroy(StartOfRound __instance)
    {
        OriginalLevels.Clear();
    }

    public static SelectableLevel AddMoonSethsFridge(SelectableLevel rend, SelectableLevel titan)
    {
        SelectableLevel newLevel = Object.Instantiate(rend);

        newLevel.levelID = BaseLevelId + 1;
        newLevel.PlanetName = "Seth's Fridge";
        newLevel.LevelDescription =
            "Welcome to the best luxury vacation planet in existence!\n" +
            "Come in and enjoy the sights and sounds inside one of the most luxurious resorts ever built by mankind.\n" +
            "We have the best french fries.\n";

        newLevel.riskLevel = "French Fry";

        newLevel.minScrap = titan.minScrap;
        newLevel.maxScrap = titan.maxScrap;
        newLevel.maxTotalScrapValue = titan.maxTotalScrapValue;
        newLevel.minTotalScrapValue = titan.minTotalScrapValue;
        newLevel.factorySizeMultiplier = titan.factorySizeMultiplier;

        newLevel.Enemies = new List<SpawnableEnemyWithRarity>(titan.Enemies);
        newLevel.DaytimeEnemies = new List<SpawnableEnemyWithRarity>(titan.DaytimeEnemies);
        newLevel.OutsideEnemies = new List<SpawnableEnemyWithRarity>(titan.OutsideEnemies);
        newLevel.daytimeEnemiesProbabilityRange = titan.daytimeEnemiesProbabilityRange;
        newLevel.spawnableScrap = new List<SpawnableItemWithRarity>(titan.spawnableScrap);

        newLevel.maxEnemyPowerCount = titan.maxEnemyPowerCount;

        return newLevel;
    }

    public static void AddCustomMoonToTerminal(
        SelectableLevel level,
        int cost,
        string keyword,
        string beforeConfirmText = "",
        string confirmedText = ""
    )
    {
        Terminal terminal = Object.FindObjectOfType<Terminal>();

        TerminalKeyword routeKeyword = terminal.terminalNodes.allKeywords.First(k => k.word == "route");
        var confirmNoun = new CompatibleNoun
        {
            noun = routeKeyword.compatibleNouns[0].result.terminalOptions[1].noun,
        };
        CompatibleNoun denyNoun = routeKeyword.compatibleNouns[0].result.terminalOptions[0];

        TerminalKeyword moonKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        moonKeyword.name = level.PlanetName;
        moonKeyword.word = keyword;
        moonKeyword.defaultVerb = routeKeyword;

        terminal.moonsCatalogueList = terminal.moonsCatalogueList.AddToArray(level);
        terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddToArray(moonKeyword);

        TerminalNode thanksNode = ScriptableObject.CreateInstance<TerminalNode>();
        thanksNode.name = $"{keyword}-purchased";
        thanksNode.displayText =
            $"Routing autopilot to {level.PlanetName}.\n" +
            "Your new balance is [playerCredits].\n\n" +
            $"{(confirmedText != null ? confirmedText + "\n\n" : "")}";
        thanksNode.buyRerouteToMoon = level.levelID;
        thanksNode.itemCost = cost;
        thanksNode.clearPreviousText = true;
        thanksNode.buyUnlockable = false;

        confirmNoun.result = thanksNode;

        TerminalNode confirmNode = ScriptableObject.CreateInstance<TerminalNode>();
        confirmNode.name = $"{keyword}-purchase-confirmation";
        confirmNode.buyItemIndex = -1;
        confirmNode.clearPreviousText = true;
        confirmNode.buyUnlockable = false;
        confirmNode.displayText =
            $"The cost to route to {level.PlanetName} is [totalCost]. " +
            "It is \ncurrently [currentPlanetTime] on this moon.\n\n " +
            $"{(beforeConfirmText != null ? beforeConfirmText + "\n\n" : "")}" +
            "Please CONFIRM or DENY.\n\n\n";
        confirmNode.isConfirmationNode = false;
        confirmNode.displayPlanetInfo = level.levelID;
        confirmNode.itemCost = cost;
        confirmNode.overrideOptions = true;
        confirmNode.maxCharactersToType = 15;
        confirmNode.terminalOptions = new[]
        {
            denyNoun,
            confirmNoun,
        };

        var moonNoun = new CompatibleNoun
        {
            noun = moonKeyword,
            result = confirmNode
        };

        Plugin.Log.LogInfo($"Adding custom moon {keyword} to terminal.");

        routeKeyword.compatibleNouns = routeKeyword.compatibleNouns.AddToArray(moonNoun);
    }
}