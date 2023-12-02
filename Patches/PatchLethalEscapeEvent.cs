﻿using System;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Patches;

[UsedImplicitly]
internal class PatchLethalEscapeEvent
{
    private static float timeStartTeleport;

    public static void SendEnemyInside(EnemyAI instance)
    {
        instance.enemyType.isOutsideEnemy = false;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
        EntranceTeleport[] array = Object.FindObjectsOfType<EntranceTeleport>(false);

        foreach (EntranceTeleport et in array)
        {
            bool flag = et.entranceId == 0 && !et.isEntranceToBuilding;
            if (flag)
            {
                instance.serverPosition = et.entrancePoint.position;
                break;
            }
        }

        Transform transform = instance.ChooseClosestNodeToPosition(instance.serverPosition);
        bool flag2 = Vector3.Magnitude(transform.position - instance.serverPosition) > 10f;
        if (flag2)
        {
            instance.serverPosition = transform.position;
            instance.transform.position = instance.serverPosition;
        }

        instance.transform.position = instance.serverPosition;
        instance.agent.Warp(instance.serverPosition);
        instance.SyncPositionToClients();
    }

    public static void SendEnemyOutside(EnemyAI instance, bool spawnOnDoor = true)
    {
        instance.enemyType.isOutsideEnemy = true;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
        EntranceTeleport[] array = Object.FindObjectsOfType<EntranceTeleport>(false);

        float num = 999f;
        for (int i = 0; i < array.Length; i++)
        {
            bool isEntranceToBuilding = array[i].isEntranceToBuilding;
            if (isEntranceToBuilding)
            {
                for (int j = 0; j < StartOfRound.Instance.connectedPlayersAmount + 1; j++)
                {
                    bool flag = !StartOfRound.Instance.allPlayerScripts[j].isInsideFactory &
                                Vector3.Magnitude(StartOfRound.Instance.allPlayerScripts[j].transform.position -
                                                  array[i].entrancePoint.position) < num;
                    if (flag)
                    {
                        num = Vector3.Magnitude(StartOfRound.Instance.allPlayerScripts[j].transform.position -
                                                array[i].entrancePoint.position);
                        instance.serverPosition = array[i].entrancePoint.position;
                    }
                }

                break;
            }
        }

        Transform transform = instance.ChooseClosestNodeToPosition(instance.serverPosition);
        bool flag2 = Vector3.Magnitude(transform.position - instance.serverPosition) > 10f || !spawnOnDoor;
        if (flag2)
        {
            instance.serverPosition = transform.position;
            instance.transform.position = instance.serverPosition;
        }

        instance.transform.position = instance.serverPosition;
        instance.agent.Warp(instance.serverPosition);
        instance.SyncPositionToClients();
        bool flag3 = GameNetworkManager.Instance.localPlayerController != null;
        if (flag3)
        {
            instance.EnableEnemyMesh(
                !StartOfRound.Instance.hangarDoorsClosed ||
                !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
        }
    }

    [HarmonyPatch(typeof(CrawlerAI), "Start")]
    [HarmonyPrefix]
    private static void CrawlerLePrefixStart(CrawlerAI instance)
    {
        instance.enemyType.isOutsideEnemy = false;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
    }

    [HarmonyPatch(typeof(CrawlerAI), "Update")]
    [HarmonyPrefix]
    private static void CrawlerLePrefixAI(CrawlerAI instance)
    {
        bool flag = !instance.enemyType.isOutsideEnemy;
        if (flag)
        {
            bool flag2 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 5.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 5.0)) < 0.001;
            if (flag2)
            {
                Transform transform = instance.ChooseClosestNodeToPosition(instance.transform.position);
                bool flag3 = Vector3.Magnitude(transform.position - instance.transform.position) > 50f;
                if (flag3)
                {
                    Debug.Log(
                        ":Lethal Escape: FAILSAFE ACTIVATED AI STATE IS INSIDE BUT THEY ARE OUTSIDE POTENTIALLY?!?!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }

            bool flag4 = instance.currentBehaviourStateIndex != 0 && !instance.enemyType.isOutsideEnemy;
            if (flag4)
            {
                bool flag5 = instance.noticePlayerTimer < -5f;
                if (flag5)
                {
                    bool flag6 = true;
                    for (int i = 0; i < StartOfRound.Instance.connectedPlayersAmount + 1; i++)
                    {
                        bool isInsideFactory = StartOfRound.Instance.allPlayerScripts[i].isInsideFactory;
                        if (isInsideFactory)
                        {
                            flag6 = false;
                        }
                    }

                    bool flag7 = flag6;
                    if (flag7)
                    {
                        SendEnemyOutside(instance);
                    }
                }
            }
        }
        else
        {
            bool flag8 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 10.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 10.0)) < 0.001;
            if (flag8)
            {
                instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                bool flag9 =
                    Vector3.Magnitude(
                        instance.ChooseClosestNodeToPosition(instance.transform.position).position -
                        instance.transform.position) > 50f;
                if (flag9)
                {
                    Debug.Log(
                        ":Lethal Escape: AI IS OUTSIDE TYPE BUT IS STILL STUCK IN FACILITY SETTING OUTSIDE!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JesterAI), "Start")]
    [HarmonyPrefix]
    private static void JesterLePrefixStart(JesterAI instance)
    {
        instance.enemyType.isOutsideEnemy = false;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
    }

    [HarmonyPatch(typeof(JesterAI), "Update")]
    [HarmonyPrefix]
    private static void JesterLePrefixAI(JesterAI instance)
    {
        bool flag = !instance.enemyType.isOutsideEnemy;
        if (flag)
        {
            bool flag2 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 5.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 5.0)) < 0.01;
            if (flag2)
            {
                Transform transform = instance.ChooseClosestNodeToPosition(instance.transform.position);
                bool flag3 = Vector3.Magnitude(transform.position - instance.transform.position) > 50f;
                if (flag3)
                {
                    Debug.Log(
                        ":Lethal Escape: FAILSAFE ACTIVATED AI STATE IS INSIDE BUT THEY ARE OUTSIDE POTENTIALLY?!?!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }

            bool flag4 = instance.currentBehaviourStateIndex == 2;
            if (flag4)
            {
                bool flag5 = instance.agent.speed == 0f && instance.stunNormalizedTimer <= 0f &&
                             instance.targetPlayer && !instance.targetPlayer.isInsideFactory;
                if (flag5)
                {
                    instance.popUpTimer = -4f;
                }

                bool flag6 = !instance.enemyType.isOutsideEnemy && Math.Abs(instance.popUpTimer - (-4f)) < 0.001 &&
                             (instance.targetPlayer == null || !instance.targetPlayer.isInsideFactory);
                if (flag6)
                {
                    instance.currentBehaviourStateIndex = 0;
                    SendEnemyOutside(instance);
                }
            }
        }
        else
        {
            bool flag7 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 10.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 10.0)) < 0.001;
            if (flag7)
            {
                instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                bool flag8 =
                    Vector3.Magnitude(
                        instance.ChooseClosestNodeToPosition(instance.transform.position).position -
                        instance.transform.position) > 50f;
                if (flag8)
                {
                    Debug.Log(
                        ":Lethal Escape: AI IS OUTSIDE TYPE BUT IS STILL STUCK IN FACILITY SETTING OUTSIDE!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }
        }
    }

    // Token: 0x06000008 RID: 8 RVA: 0x00002555 File Offset: 0x00000755
    [HarmonyPatch(typeof(FlowermanAI), "Start")]
    [HarmonyPrefix]
    private static void FlowermanAilePrefixStart(FlowermanAI instance)
    {
        timeStartTeleport = 0f;
        instance.enemyType.isOutsideEnemy = false;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
    }

    // Token: 0x06000009 RID: 9 RVA: 0x00002580 File Offset: 0x00000780
    [HarmonyPatch(typeof(FlowermanAI), "DoAIInterval")]
    [HarmonyPrefix]
    private static void FlowermanAilePrefixAI(FlowermanAI instance)
    {
        bool flag = !instance.enemyType.isOutsideEnemy;
        if (flag)
        {
            bool flag2 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 5.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 5.0)) < 0.01;
            if (flag2)
            {
                Transform transform = instance.ChooseClosestNodeToPosition(instance.transform.position);
                bool flag3 = Vector3.Magnitude(transform.position - instance.transform.position) > 50f;
                if (flag3)
                {
                    Debug.Log(
                        ":Lethal Escape: FAILSAFE ACTIVATED AI STATE IS INSIDE BUT THEY ARE OUTSIDE POTENTIALLY?!?!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }

            bool flag4 = instance.targetPlayer && !instance.targetPlayer.isInsideFactory &&
                         (instance.currentBehaviourStateIndex == 1 || instance.evadeStealthTimer > 0f);
            if (flag4)
            {
                bool flag5 = Time.time - timeStartTeleport >= 10f;
                if (flag5)
                {
                    timeStartTeleport = Time.time;
                }
            }

            bool flag6 = Time.time - timeStartTeleport > 5f && Time.time - timeStartTeleport < 10f;
            if (flag6)
            {
                SendEnemyOutside(instance);
            }
        }
        else
        {
            bool flag7 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 10.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 10.0)) < 0.01;
            if (flag7)
            {
                instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                bool flag8 =
                    Vector3.Magnitude(
                        instance.ChooseClosestNodeToPosition(instance.transform.position).position -
                        instance.transform.position) > 50f;
                if (flag8)
                {
                    Debug.Log(
                        ":Lethal Escape: AI IS OUTSIDE TYPE BUT IS STILL STUCK IN FACILITY SETTING OUTSIDE!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }
        }
    }

    // Token: 0x0600000A RID: 10 RVA: 0x000027B6 File Offset: 0x000009B6
    [HarmonyPatch(typeof(HoarderBugAI), "Start")]
    [HarmonyPrefix]
    private static void HoarderBugAilePrefixStart(HoarderBugAI instance)
    {
        instance.enemyType.isOutsideEnemy = false;
        instance.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
    }

    // Token: 0x0600000B RID: 11 RVA: 0x000027D8 File Offset: 0x000009D8
    [HarmonyPatch(typeof(HoarderBugAI), "DoAIInterval")]
    [HarmonyPrefix]
    private static void HoardingBugAilePrefixAI(HoarderBugAI instance)
    {
        bool flag = !instance.enemyType.isOutsideEnemy;
        if (flag)
        {
            bool flag2 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 5.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 5.0)) < 0.001;
            if (flag2)
            {
                Transform transform = instance.ChooseClosestNodeToPosition(instance.transform.position);
                bool flag3 = Vector3.Magnitude(transform.position - instance.transform.position) > 50f;
                if (flag3)
                {
                    Debug.Log(
                        ":Lethal Escape: FAILSAFE ACTIVATED AI STATE IS INSIDE BUT THEY ARE OUTSIDE POTENTIALLY?!?!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }

            bool flag4 = !instance.targetPlayer.isInsideFactory && instance.searchForPlayer.inProgress;
            if (flag4)
            {
                SendEnemyOutside(instance);
            }
        }
        else
        {
            bool flag5 = Math.Round(Time.time / 0.01) * 0.01 / 10.0 ==
                         Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 10.0);
            if (flag5)
            {
                instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                bool flag6 =
                    Vector3.Magnitude(
                        instance.ChooseClosestNodeToPosition(instance.transform.position).position -
                        instance.transform.position) > 50f;
                if (flag6)
                {
                    Debug.Log(
                        ":Lethal Escape: AI IS OUTSIDE TYPE BUT IS STILL STUCK IN FACILITY SETTING OUTSIDE!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }
        }
    }

    // Token: 0x0600000C RID: 12 RVA: 0x000029A0 File Offset: 0x00000BA0
    [HarmonyPatch(typeof(SpringManAI), "DoAIInterval")]
    [HarmonyPrefix]
    private static void CoilHeadAilePrefixAI(SpringManAI instance)
    {
        bool flag = !instance.enemyType.isOutsideEnemy;
        if (flag)
        {
            bool flag2 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 5.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 5.0)) < 0.001;
            if (flag2)
            {
                Transform transform = instance.ChooseClosestNodeToPosition(instance.transform.position);
                bool flag3 = Vector3.Magnitude(transform.position - instance.transform.position) > 50f;
                if (flag3)
                {
                    Debug.Log(
                        ":Lethal Escape: FAILSAFE ACTIVATED AI STATE IS INSIDE BUT THEY ARE OUTSIDE POTENTIALLY?!?!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }

            bool flag4 = instance.targetPlayer != null && !instance.targetPlayer.isInsideFactory;
            if (flag4)
            {
                SendEnemyOutside(instance, false);
            }
        }
        else
        {
            bool flag5 = Math.Abs(Math.Round(Time.time / 0.01) * 0.01 / 10.0 -
                                  Math.Round(Math.Round(Time.time / 0.01) * 0.01 / 10.0)) < 0.001;
            if (flag5)
            {
                instance.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                bool flag6 =
                    Vector3.Magnitude(
                        instance.ChooseClosestNodeToPosition(instance.transform.position).position -
                        instance.transform.position) > 50f;
                if (flag6)
                {
                    Debug.Log(
                        ":Lethal Escape: AI IS OUTSIDE TYPE BUT IS STILL STUCK IN FACILITY SETTING OUTSIDE!!!!!!!!!!!!!!!!!!!!!!///////////////////////////////////////////////////////////////");
                    SendEnemyOutside(instance);
                }
            }
        }
    }
}