using GameNetcodeStuff;

namespace FishsGrandAdventure.Behaviors;

public class PlayerEffectHelium : Effect
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