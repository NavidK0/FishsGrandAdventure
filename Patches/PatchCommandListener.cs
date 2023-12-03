using System;
using System.Linq;
using FishsGrandAdventure.Game;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FishsGrandAdventure.Patches;

internal class PatchCommandListener
{
    public const string CommandPrefix = "!";

    [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool ListenForCommands(HUDManager __instance, ref InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return true;
        }

        if (string.IsNullOrEmpty(__instance.chatTextField.text))
        {
            return true;
        }

        PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
        if (localPlayerController == null)
        {
            return true;
        }

        StartOfRound playersManager = localPlayerController.playersManager;
        if (playersManager == null)
        {
            return true;
        }

        string text = __instance.chatTextField.text;

        if (text.StartsWith(CommandPrefix))
        {
            ResetTextbox(__instance, localPlayerController);
            ParseCommand(text);

            return false;
        }

        return true;
    }

    private static void ParseCommand(string rawCmd)
    {
        string[] cmdWithArgs = rawCmd.Split(' ');
        string cmd = cmdWithArgs[0];
        string[] args = cmdWithArgs.Skip(1).ToArray();

        switch (cmd.ToLower())
        {
            case "!help":
            {
                SendChatMessage("Available commands: !help, !ping, !restart, !setEvent, !events");
                return;
            }

            case "!restart":
            {
                PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
                StartOfRound playersManager = localPlayerController.playersManager;

                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can restart.");
                    return;
                }

                if (!localPlayerController.isInHangarShipRoom || !playersManager.inShipPhase ||
                    playersManager.travellingToNewLevel)
                {
                    SendChatMessage("Cannot restart, ship must be in orbit.");
                    return;
                }

                SendChatMessage("Restart confirmed.");

                StartOfRound startOfRound = StartOfRound.Instance;

                int[] array =
                {
                    startOfRound.gameStats.daysSpent,
                    startOfRound.gameStats.scrapValueCollected,
                    startOfRound.gameStats.deaths,
                    startOfRound.gameStats.allStepsTaken
                };

                startOfRound.FirePlayersAfterDeadlineClientRpc(array);

                return;
            }

            case "!ping":
            {
                SendChatMessage("Pong!");
                return;
            }

            case "!setevent":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can set events.");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("You need to specify an event name!");
                    return;
                }

                if (Enum.TryParse(args[0], true, out GameEvent @event))
                {
                    GameState.ShouldForceLoadEvent = true;
                    GameState.ForceLoadEvent = @event;

                    HUDManager.Instance.AddTextToChatOnServer($"Forcing event {@event.ToString()} to load!");
                }

                return;
            }

            case "!events":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can see events.");
                    return;
                }

                SendChatMessage(string.Join(", ", Enum.GetNames(typeof(GameEvent))));
                return;
            }
        }
    }

    private static void ResetTextbox(HUDManager manager, PlayerControllerB local)
    {
        local.isTypingChat = false;
        manager.chatTextField.text = "";

        EventSystem.current.SetSelectedGameObject(null);

        manager.PingHUDElement(manager.Chat);
        manager.typingIndicator.enabled = false;
    }

    public static void SendChatMessage(string message)
    {
        Plugin.Chat?.Invoke(HUDManager.Instance, new object[] { message, "" });
        HUDManager.Instance.lastChatMessage = "";
    }
}