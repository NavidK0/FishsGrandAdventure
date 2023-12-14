using System;
using System.Linq;
using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Network;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game;

internal class CommandListener
{
    public const string CommandPrefix = "!";

    [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
    [HarmonyPrefix]
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
                SendChatMessage(
                    "Available commands: !help, !ping, !restart, !setevent, !skip, !events, !setcredits, !debug, !playmusic, !stopmusic, !audioclips"
                );
                return;
            }

            case "!restart":
            {
                PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
                StartOfRound playersManager = localPlayerController.playersManager;

                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this.");
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
                    SendChatMessage("Only the host can do this.");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("!setevent <event>");
                    return;
                }

                if (Enum.TryParse(args[0], true, out GameEventType @event))
                {
                    GameState.ForceLoadEvent = @event;

                    HUDManager.Instance.AddTextToChatOnServer($"Forcing event {@event.ToString()} to load!");

                    GameEventManager.SetupNewEvent();
                }

                return;
            }

            case "!skip":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this.");
                    return;
                }

                HUDManager.Instance.AddTextToChatOnServer("Skipping event!");

                GameState.ForceLoadEvent = null;
                GameEventManager.SetupNewEvent();

                return;
            }

            case "!events":
            {
                SendChatMessage(string.Join(", ", Enum.GetNames(typeof(GameEventType))));
                return;
            }

            case "!setcredits":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this.");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("!setcredits <amount>");
                    return;
                }

                if (int.TryParse(args[0], out int val))
                {
                    Terminal terminal3 = Object.FindObjectOfType<Terminal>();
                    terminal3.groupCredits = val;
                    terminal3.SyncGroupCreditsServerRpc(terminal3.groupCredits, terminal3.numberOfItemsInDropship);

                    HUDManager.Instance.AddTextToChatOnServer($"Setting credits to {val}!");
                }

                return;
            }

            case "!debug":
            {
                SelectableLevel level = RoundManager.Instance.currentLevel;

                Plugin.Log.LogError($"Level Debug: {JsonConvert.SerializeObject(level,
                    new JsonSerializerSettings(NetworkUtils.SerializerSettings)
                    {
                        Formatting = Formatting.Indented
                    })}");

                SendChatMessage("Check the logs for a detailed debug message!");
                return;
            }

            case "!playmusic":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this.");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("!playmusic <name> [volume] [pitch] [loop]");
                    return;
                }

                NetworkUtils.BroadcastAll(new PacketPlayMusic
                {
                    Name = args[0],
                    Volume = args.Length > 1 ? float.Parse(args[1]) : .85f,
                    Pitch = args.Length > 2 ? float.Parse(args[2]) : 1f,
                    Loop = args.Length > 3 && bool.Parse(args[3])
                });

                return;
            }

            case "!stopmusic":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this.");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("!stopmusic [shouldFadeOut] [fadeOutDuration]");
                    return;
                }

                NetworkUtils.BroadcastAll(new PacketStopMusic
                {
                    FadeOut = args.Length > 0 && bool.Parse(args[0]),
                    FadeOutDuration = args.Length > 1 ? float.Parse(args[1]) : 2f,
                });

                return;
            }

            case "!audioclips":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this");
                    return;
                }

                SendChatMessage(AudioManager.LoadedAudio.Keys.Aggregate("Audio clips: ",
                    (current, key) => $"{current}{key}, "));

                return;
            }

            case "!settime":
            {
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    SendChatMessage("Only the host can do this");
                    return;
                }

                if (args.Length == 0)
                {
                    SendChatMessage("!settime [time]");
                    return;
                }

                if (float.TryParse(args[0], out float time))
                {
                    TimeOfDay.Instance.globalTime = time;
                }

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