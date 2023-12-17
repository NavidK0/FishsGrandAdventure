using System.Collections.Generic;
using UnityEngine;

namespace FishsGrandAdventure.Utils;

public static class SoundUtils
{
    /**
     * Gets all movement SFX for a specific enemy type.
     */
    public static List<AudioClip> GetAllSFX(this EnemyAI enemyAI)
    {
        List<AudioClip> allAudioClips = new List<AudioClip>();

        switch (enemyAI)
        {
            case BaboonBirdAI ai:
            {
                allAudioClips.AddRange(ai.cawLaughSFX);
                allAudioClips.AddRange(ai.cawScreamSFX);
                allAudioClips.AddRange(ai.cawScreamSFX);
                break;
            }

            case BlobAI ai:
            {
                allAudioClips.Add(ai.agitatedSFX);
                allAudioClips.Add(ai.jiggleSFX);
                allAudioClips.Add(ai.hitSlimeSFX);
                allAudioClips.Add(ai.killPlayerSFX);
                allAudioClips.Add(ai.idleSFX);
                break;
            }

            case CentipedeAI ai:
            {
                allAudioClips.Add(ai.fallShriek);
                allAudioClips.Add(ai.hitGroundSFX);
                allAudioClips.Add(ai.hitCentipede);
                allAudioClips.AddRange(ai.shriekClips);
                allAudioClips.Add(ai.clingToPlayer3D);
                break;
            }

            case CrawlerAI ai:
            {
                allAudioClips.Add(ai.shortRoar);
                allAudioClips.AddRange(ai.hitWallSFX);
                allAudioClips.AddRange(ai.hitCrawlerSFX);
                allAudioClips.Add(ai.bitePlayerSFX);
                allAudioClips.Add(ai.eatPlayerSFX);
                allAudioClips.AddRange(ai.hitCrawlerSFX);
                allAudioClips.AddRange(ai.longRoarSFX);
                break;
            }

            case DoublewingAI ai:
            {
                allAudioClips.AddRange(ai.birdScreechSFX);
                allAudioClips.Add(ai.birdHitGroundSFX);
                break;
            }

            case DressGirlAI ai:
            {
                allAudioClips.AddRange(ai.appearStaringSFX);
                allAudioClips.Add(ai.skipWalkSFX);
                allAudioClips.Add(ai.breathingSFX);
                break;
            }

            case FlowermanAI ai:
            {
                allAudioClips.Add(ai.crackNeckSFX);
                break;
            }

            case HoarderBugAI ai:
            {
                allAudioClips.AddRange(ai.chitterSFX);
                allAudioClips.AddRange(ai.angryScreechSFX);
                allAudioClips.Add(ai.angryVoiceSFX);
                allAudioClips.Add(ai.bugFlySFX);
                allAudioClips.Add(ai.hitPlayerSFX);
                break;
            }

            case JesterAI ai:
            {
                allAudioClips.Add(ai.popGoesTheWeaselTheme);
                allAudioClips.Add(ai.popUpSFX);
                allAudioClips.Add(ai.screamingSFX);
                allAudioClips.Add(ai.killPlayerSFX);
                break;
            }

            case MouthDogAI ai:
            {
                allAudioClips.Add(ai.screamSFX);
                allAudioClips.Add(ai.breathingSFX);
                allAudioClips.Add(ai.killPlayerSFX);
                break;
            }

            case NutcrackerEnemyAI ai:
            {
                allAudioClips.AddRange(ai.torsoFinishTurningClips);
                allAudioClips.Add(ai.aimSFX);
                allAudioClips.Add(ai.kickSFX);
                break;
            }

            case PufferAI ai:
            {
                allAudioClips.AddRange(ai.footstepsSFX);
                allAudioClips.AddRange(ai.frightenSFX);
                allAudioClips.Add(ai.stomp);
                allAudioClips.Add(ai.angry);
                allAudioClips.Add(ai.puff);
                allAudioClips.Add(ai.nervousMumbling);
                allAudioClips.Add(ai.rattleTail);
                allAudioClips.Add(ai.bitePlayerSFX);
                break;
            }

            case SandSpiderAI ai:
            {
                allAudioClips.AddRange(ai.footstepSFX);
                allAudioClips.Add(ai.hitWebSFX);
                allAudioClips.Add(ai.attackSFX);
                allAudioClips.Add(ai.spoolPlayerSFX);
                allAudioClips.Add(ai.hangPlayerSFX);
                allAudioClips.Add(ai.breakWebSFX);
                allAudioClips.Add(ai.hitSpiderSFX);
                break;
            }

            case SandWormAI ai:
            {
                allAudioClips.AddRange(ai.groundRumbleSFX);
                allAudioClips.AddRange(ai.ambientRumbleSFX);
                allAudioClips.Add(ai.hitGroundSFX);
                allAudioClips.Add(ai.emergeFromGroundSFX);
                allAudioClips.AddRange(ai.roarSFX);
                break;
            }

            case SpringManAI ai:
            {
                allAudioClips.AddRange(ai.springNoises);
                break;
            }
        }

        return allAudioClips;
    }
}