using FishsGrandAdventure.Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishsGrandAdventure.Behaviors.Wendigo;

public class WendigoMimicry : Effect
{
    private AudioSource audioSource;

    private const float PLAY_INTERVAL_MIN = 15f;
    private const float PLAY_INTERVAL_MAX = 40f;
    private const float VOICE_LINE_FREQUENCY = 1f;

    private const float MAX_DIST = 100f;

    private float nextTimeToPlayAudio;

    private EnemyAI ai;

    public void Initialize(EnemyAI ai)
    {
        this.ai = ai;
        audioSource = ai.creatureVoice;
        SetNextTime();
    }

    private void Update()
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.Wendigo)
        {
            return;
        }

        if (!(Time.time > nextTimeToPlayAudio))
        {
            return;
        }

        SetNextTime();

        if (ai != null && !ai.isEnemyDead)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null ||
                (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position,
                    transform.position)) < MAX_DIST)
            {
                AudioClip sample = WendigoRecorder.Instance.GetSample();
                if (sample != null)
                {
                    audioSource.PlayOneShot(sample);
                }
            }
        }
    }

    private void SetNextTime()
    {
        nextTimeToPlayAudio = Time.time +
                              Random.Range(PLAY_INTERVAL_MIN, PLAY_INTERVAL_MAX) / VOICE_LINE_FREQUENCY;
    }
}