using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Effects;

public class PlayerEffectBlazed : MonoBehaviour
{
    public PlayerControllerB PlayerController;

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
        PlayerController.drunknessInertia = 0f;
        PlayerController.drunkness = 4f;
    }
}