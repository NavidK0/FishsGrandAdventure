using System;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Network;
using MEC;
using Unity.Netcode;
using UnityEngine;

namespace FishsGrandAdventure.Effects;

public class ExplodingJester : MonoBehaviour
{
    private JesterAI jesterAI;
    private bool jesterExploding;

    private void Awake()
    {
        jesterAI = GetComponentInChildren<JesterAI>();

        if (jesterAI == null)
        {
            Plugin.Log.LogInfo("Attempted to add JesterEffectExplodeOnFinish to a non-jester AI!");
            Destroy(this);
        }
    }

    private void LateUpdate()
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.ClownExpo ||
            !RoundManager.Instance.IsServer ||
            jesterAI.isEnemyDead || jesterExploding
           ) return;

        bool explodingJester = jesterAI.GetComponent<ExplodingJester>() != null;

        if (explodingJester && jesterAI != null)
        {
            jesterAI.agent.speed = 5f;

            if (jesterAI.currentBehaviourStateIndex == 2)
            {
                Timing.CallDelayed(.5f, () =>
                {
                    NetworkUtils.BroadcastAll(new PacketSpawnExplosion
                    {
                        Position = jesterAI.transform.position,
                        DamageRange = 10f,
                        KillRange = 10f
                    });

                    jesterAI.agent.speed = 0f;
                    jesterAI.isEnemyDead = true;

                    jesterAI.GetComponentInChildren<NetworkObject>().Despawn();
                });

                jesterExploding = true;
            }
        }
    }
}