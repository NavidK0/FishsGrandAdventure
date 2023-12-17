using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Behaviors;

public class PlayerEffectBlazed : Effect
{
    private const float CooldownTime = 60;
    private const float InitialFuelValue = 1.3f;
    private const float MaxDrunkness = 0.7f;

    public PlayerControllerB PlayerController;

    private float cooldown;

    private float fuelValue = InitialFuelValue;

    private bool musicStarted;
    private bool musicFadingOut = true;

    private void Start()
    {
        PlayerController = StartOfRound.Instance.localPlayerController;
    }

    private void OnDestroy()
    {
        PlayerController.drunkness = 0f;
        PlayerController.drunknessInertia = 0f;

        foreach (PlayerControllerB playerController in StartOfRound.Instance.allPlayerScripts)
        {
            if (playerController.currentVoiceChatAudioSource != null)
            {
                playerController.currentVoiceChatAudioSource.pitch = 1f;
            }
        }

        AudioManager.MusicSource.Stop();
    }

    private void LateUpdate()
    {
        PlayerController.drunkness = Mathf.Clamp(PlayerController.drunkness, 0, MaxDrunkness);

        if (PlayerController.drunkness > 0f)
        {
            for (var index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; index++)
            {
                SoundManager.Instance.SetPlayerPitch(
                    Mathf.Clamp(
                        Mathf.Clamp(PlayerController.drunkness, 0, MaxDrunkness)
                            .Remap(0f, MaxDrunkness, 1f, 1.5f),
                        1f,
                        2f),
                    index);
            }
        }
        else
        {
            for (var index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; index++)
            {
                SoundManager.Instance.SetPlayerPitch(1f, index);
            }
        }

        if (!musicFadingOut &&
            AudioManager.MusicSource.isPlaying &&
            PlayerController.drunkness <= 0.15f)
        {
            AudioManager.StopMusic(true, 2f);
            musicFadingOut = true;
            cooldown = CooldownTime;
        }

        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        if (fuelValue > 0f)
        {
            PlayerController.drunknessInertia =
                Mathf.Clamp(
                    PlayerController.drunknessInertia +
                    Time.deltaTime / 1.75f * PlayerController.drunknessSpeed, 0.1f, 3f);
            PlayerController.increasingDrunknessThisFrame = true;

            if (!AudioManager.MusicSource.isPlaying && !musicStarted && PlayerController.drunkness > 0.15f)
            {
                AudioManager.PlayMusic("earthbound_baawo", .6f);

                musicStarted = true;
                musicFadingOut = false;
            }

            fuelValue -= Time.deltaTime / 44f;
        }

        if (cooldown <= 0f && fuelValue <= 0f)
        {
            cooldown = CooldownTime;
            fuelValue = InitialFuelValue;

            musicStarted = false;
        }
    }
}