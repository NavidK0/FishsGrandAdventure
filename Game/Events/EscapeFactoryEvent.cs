using System;
using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game.Events;

public class EscapeFactoryEvent : BaseGameEvent
{
    public const float MaxTime = 240;
    public const float InvincibilityGracePeriod = 5f;

    public static readonly string[] SongList =
    {
        "ahit_collapsing_time_rift"
    };

    private static readonly int PlayerControllerCancelHoldingAnimation = Animator.StringToHash("cancelHolding");
    private static readonly int PlayerControllerThrowAnimation = Animator.StringToHash("Throw");

    public static float GracePeriodEndTime;

    private static float timeLeft;
    private static Item flashlightItem;
    private static bool playedWinLossMusic;
    private static CoroutineHandle earthquakeShakeCoroutine;
    private static CoroutineHandle setDayTimerCoroutine;
    private static CoroutineHandle stopMusicCoroutine;

    public override string Name => "The Great Factory Escape";
    public override string Description => "Oh SHIT, THE FACTORY IS COLLAPSING! GET OUT NOW!";
    public override Color Color => new Color(0.99f, 0.04f, 0f, 0.43f);
    public override GameEventType GameEventType => GameEventType.EscapeFactory;

    public override void OnServerInitialize(SelectableLevel level)
    {
        flashlightItem =
            StartOfRound.Instance.allItemsList.itemsList.Find(i => i.itemName.ToLower() == "pro-flashlight");
    }

    public override void OnPreGenerateLevel(int randomSeed, int levelID)
    {
        SelectableLevel currentLevel = StartOfRound.Instance.levels[levelID];
        currentLevel.factorySizeMultiplier = 3.525f;
    }

    public override void OnPreRoundStart()
    {
        ShipTeleporter[] shipTeleporters = Object.FindObjectsOfType<ShipTeleporter>(true);
        foreach (ShipTeleporter shipTeleporter in shipTeleporters)
        {
            shipTeleporter.buttonTrigger.gameObject.SetActive(false);
        }
    }

    public override void OnPostRoundStart()
    {
        PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
        timeLeft = MaxTime;

        // Make sure all doors are unlocked on level
        foreach (DoorLock doorLock in Object.FindObjectsOfType<DoorLock>(true))
        {
            doorLock.UnlockDoor();
        }

        foreach (DoorLock doorLock in Object.FindObjectsOfType<DoorLock>(true))
        {
            doorLock.UnlockDoor();
        }

        foreach (TerminalAccessibleObject tao in Object.FindObjectsOfType<TerminalAccessibleObject>(true))
        {
            tao.SetDoorOpen(true);
            tao.SetDoorOpenServerRpc(true);
        }

        foreach (PowerSwitchable powerSwitchable in Object.FindObjectsOfType<PowerSwitchable>(true))
        {
            powerSwitchable.OnPowerSwitch(true);
        }

        ShipTeleporter[] shipTeleporters = Object.FindObjectsOfType<ShipTeleporter>(true);
        foreach (ShipTeleporter shipTeleporter in shipTeleporters)
        {
            if (!shipTeleporter.isInverseTeleporter)
            {
                shipTeleporter.buttonTrigger.gameObject.SetActive(true);
            }
        }

        CommandListener.SendChatMessage("<color=red>THE FACTORY IS COLLAPSING, GET OUT NOW!!! </color>");
        GracePeriodEndTime = (float)Time.timeAsDouble + InvincibilityGracePeriod + 1f;

        Timing.CallDelayed(.25f, () =>
        {
            if (RoundManager.Instance.IsServer)
            {
                for (var i = 0; i < GameNetworkManager.Instance.connectedPlayers; i++)
                {
                    PlayerControllerB playerController = players[i];
                    TeleportPlayerIntoFactory(playerController);
                }

                NetworkUtils.BroadcastAll(new PacketEscapeFactoryStartEvent());
            }
        });
    }

    public override void Cleanup()
    {
        AudioManager.MusicSource.Stop();
        playedWinLossMusic = false;

        ShipTeleporter[] shipTeleporters = Object.FindObjectsOfType<ShipTeleporter>(true);
        foreach (ShipTeleporter shipTeleporter in shipTeleporters)
        {
            shipTeleporter.buttonTrigger.gameObject.SetActive(true);
        }

        Timing.KillCoroutines(earthquakeShakeCoroutine);
        Timing.KillCoroutines(setDayTimerCoroutine);
        Timing.KillCoroutines(stopMusicCoroutine);
    }

    public static void StartEvent()
    {
        AudioManager.PlaySFX("mc_nether_portal", .5f);

        NetworkUtils.BroadcastAll(new PacketPlayMusic
        {
            Name = SongList.GetRandomElement(),
            Volume = .5f
        });

        stopMusicCoroutine = Timing.CallDelayed(MaxTime - 10f, () =>
        {
            NetworkUtils.BroadcastAll(new PacketStopMusic
            {
                FadeOut = true,
                FadeOutDuration = 2f
            });
        });

        earthquakeShakeCoroutine =
            Timing.CallPeriodically(MaxTime, 10, () =>
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                AudioManager.PlaySFX("earthquake_rumble", .7f);
            });

        setDayTimerCoroutine =
            Timing.CallContinuously(MaxTime, () =>
                {
                    var time = TimeSpan.FromSeconds(timeLeft);

                    HUDManager.Instance.clockNumber.text = $"{time:mm\\:ss} LEFT";
                    HUDManager.Instance.SetClockIcon(DayMode.Midnight);
                    HUDManager.Instance.SetClockVisible(true);

                    float deltaTime = Timing.DeltaTime;
                    timeLeft -= deltaTime;

                    if (StartOfRound.Instance.localPlayerController.isPlayerDead &&
                        AudioManager.MusicSource.isPlaying &&
                        playedWinLossMusic
                       )
                    {
                        AudioManager.MusicSource.Stop();
                        AudioManager.PlaySFX("mk64_lost", .75f);
                        playedWinLossMusic = true;
                    }

                    if (!StartOfRound.Instance.localPlayerController.isInsideFactory &&
                        playedWinLossMusic)
                    {
                        AudioManager.PlaySFX("smk_win", .75f);
                        playedWinLossMusic = true;
                    }

                    var playersInFactory = false;
                    var playerStillAlive = false;

                    for (var i = 0; i < GameNetworkManager.Instance.connectedPlayers; i++)
                    {
                        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];

                        if (playerController.isInsideFactory)
                        {
                            playersInFactory = true;
                        }

                        if (!playerController.isPlayerDead)
                        {
                            playerStillAlive = true;
                        }
                    }

                    if (!playersInFactory || !playerStillAlive)
                    {
                        if (RoundManager.Instance.IsServer)
                        {
                            Timing.RunCoroutine(FinishUp());
                        }

                        Timing.KillCoroutines(setDayTimerCoroutine);
                        Timing.KillCoroutines(earthquakeShakeCoroutine);
                    }
                },
                Segment.LateUpdate,
                () =>
                {
                    PlayerControllerB playerController = StartOfRound.Instance.localPlayerController;

                    if (!playerController.isPlayerDead) return;

                    if (playerController.isInsideFactory)
                    {
                        NetworkUtils.BroadcastAll(new PacketSpawnExplosion
                        {
                            Position = playerController.transform.position
                        });
                    }

                    Timing.CallDelayed(1f, () =>
                    {
                        if (RoundManager.Instance.IsServer)
                        {
                            Timing.RunCoroutine(FinishUp());
                        }

                        Timing.KillCoroutines(earthquakeShakeCoroutine);
                    });
                });
    }

    private static IEnumerator<float> FinishUp()
    {
        NetworkUtils.BroadcastAll(new PacketStopMusic
        {
            FadeOut = true
        });

        yield return Timing.WaitForSeconds(2f);

        var playerStillAlive = false;
        var allPlayersAlive = true;

        for (var i = 0; i < GameNetworkManager.Instance.connectedPlayers; i++)
        {
            PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];

            if (!playerController.isPlayerDead)
            {
                playerStillAlive = true;
            }
            else
            {
                allPlayersAlive = false;
            }
        }

        if (allPlayersAlive)
        {
            NetworkUtils.BroadcastAll(new PacketPlaySFX
            {
                Name = "mk64_finish",
                Volume = .75f
            });
            NetworkUtils.BroadcastAll(new PacketPlaySFX
            {
                Name = "crowd_cheering",
                Volume = .75f
            });
        }
        else if (playerStillAlive)
        {
            NetworkUtils.BroadcastAll(new PacketPlaySFX
            {
                Name = "mk64_finish",
                Volume = .75f
            });
        }
        else
        {
            NetworkUtils.BroadcastAll(new PacketPlaySFX
            {
                Name = "smk_game_over",
                Volume = .75f
            });
        }
    }

    private static void TeleportPlayerIntoFactory(PlayerControllerB playerController)
    {
        HangarShipDoor shipDoor = Object.FindObjectOfType<HangarShipDoor>();

        Vector3 spawnPos = RoundManager.Instance
            .insideAINodes
            .OrderByDescending(aiNode => Vector3.Distance(shipDoor.transform.position, aiNode.transform.position))
            .First()
            .transform.position;

        spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(spawnPos);

        NetworkUtils.BroadcastAll(new PacketTeleportPlayerIntoFactory
        {
            ClientId = playerController.actualClientId,
            Position = spawnPos
        });

        GameObject go = Object.Instantiate(flashlightItem.spawnPrefab,
            spawnPos,
            Quaternion.identity
        );

        FlashlightItem grabbable = go.GetComponent<FlashlightItem>();
        grabbable.fallTime = 0f;
        grabbable.flashlightBulb.range *= 2f;
        grabbable.SetScrapValue(150);

        NetworkObject gunNetObject = go.GetComponent<NetworkObject>();
        gunNetObject.Spawn();
    }

    public static void SendToShip(
        PlayerControllerB playerController,
        GrabbableObject grabbableObject
    )
    {
        Transform teleportLocation = StartOfRound.Instance.playerSpawnPositions.GetRandomElement();
        Vector3 teleportPos = teleportLocation.position;

        Vector3 placePos = StartOfRound.Instance.propsContainer.InverseTransformPoint(teleportPos);
        placePos.y += 0.25f;

        Timing.CallDelayed(.25f, () =>
        {
            if (playerController.beamOutParticle.isPlaying || playerController.beamUpParticle.isPlaying)
            {
                return;
            }

            int slotIndex =
                playerController.ItemSlots.FindIndex(
                    slot => slot.NetworkObjectId == grabbableObject.NetworkObjectId
                );

            playerController.SetSpecialGrabAnimationBool(false, grabbableObject);
            playerController.playerBodyAnimator.SetBool(PlayerControllerCancelHoldingAnimation, true);
            playerController.playerBodyAnimator.SetTrigger(PlayerControllerThrowAnimation);

            HUDManager.Instance.itemSlotIcons[slotIndex].enabled = false;
            HUDManager.Instance.holdingTwoHandedItem.enabled = false;

            playerController.SetThrowingObject(true);

            playerController.SetObjectAsNoLongerHeld(
                false,
                true,
                placePos,
                grabbableObject,
                (int)teleportPos.y
            );
            grabbableObject.DiscardItemOnClient();

            playerController.ThrowObjectServerRpc(
                grabbableObject.NetworkObject,
                false,
                true,
                placePos,
                (int)teleportPos.y
            );
        });
    }
}

internal static class PatchEscapeFactoryEvent
{
    [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectServerRpc")]
    [HarmonyPostfix]
    private static void GrabObjectServerRpc(PlayerControllerB __instance, ref NetworkObjectReference grabbedObject)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.EscapeFactory) return;
        if (!__instance.isInsideFactory) return;

        if (grabbedObject.TryGet(out NetworkObject networkedObject))
        {
            GrabbableObject grabbableObject = networkedObject.GetComponent<GrabbableObject>();
            bool isScrap = grabbableObject.itemProperties.isScrap;

            if (isScrap)
            {
                EscapeFactoryEvent.SendToShip(__instance, grabbableObject);
            }
        }
    }

    [HarmonyPatch(typeof(GrabbableObject), "LateUpdate")]
    [HarmonyPostfix]
    private static void OnGrabbableObjectLateUpdate(GrabbableObject __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.EscapeFactory) return;

        bool isFlashlight = __instance.itemProperties.itemName.ToLower().Contains("pro-flashlight");
        if (!isFlashlight) return;

        if (__instance.insertedBattery == null)
        {
            __instance.insertedBattery = new Battery(false, 1f);
        }
        else
        {
            __instance.insertedBattery.empty = false;
            __instance.insertedBattery.charge = 1f;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPostfix]
    private static void PlayerControllerBUpdate(PlayerControllerB __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.EscapeFactory) return;

        __instance.sprintMeter = 1f;

        if (EscapeFactoryEvent.GracePeriodEndTime > (float)Time.timeAsDouble)
        {
            __instance.health = 100;
        }
    }

    [HarmonyPatch(typeof(EntranceTeleport), "Update")]
    [HarmonyPostfix]
    private static void OnPostEntranceTeleportUpdate(EntranceTeleport __instance, InteractTrigger ___triggerScript)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.EscapeFactory) return;

        PlayerControllerB playerController = StartOfRound.Instance.localPlayerController;

        if (!playerController.isInsideFactory && ___triggerScript.interactable)
        {
            ___triggerScript.interactable = false;
        }
        else
        {
            ___triggerScript.interactable = true;
        }
    }
}