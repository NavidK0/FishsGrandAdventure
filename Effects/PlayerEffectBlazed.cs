using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Effects;

public class PlayerEffectBlazed : Effect
{
    private const float CooldownTime = 10f;

    public PlayerControllerB PlayerController;

    private float cooldown;

    private float fuelValue = 1f;

    private void Start()
    {
        PlayerController = StartOfRound.Instance.localPlayerController;
    }

    private void OnDestroy()
    {
        PlayerController.drunkness = 0f;
        PlayerController.drunknessInertia = 0f;
    }

    private void LateUpdate()
    {
        if (PlayerController.drunkness > 0f)
        {
            return;
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

            fuelValue -= Time.deltaTime / 22f;
        }

        if (cooldown <= 0f && fuelValue <= 0f)
        {
            cooldown = CooldownTime;
            fuelValue = 1f;
        }
    }
}