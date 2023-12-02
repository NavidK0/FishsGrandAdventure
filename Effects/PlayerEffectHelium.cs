using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Effects;

public class PlayerEffectHelium : MonoBehaviour
{
    public PlayerControllerB PlayerController;

    private void OnDestroy()
    {
        ulong clientId = PlayerController.playerClientId;
        SoundManager.Instance.playerVoicePitchTargets[clientId] = 1f;
    }

    private void LateUpdate()
    {
        ulong clientId = PlayerController.playerClientId;
        SoundManager.Instance.playerVoicePitchTargets[clientId] = 1.75f;
    }
}