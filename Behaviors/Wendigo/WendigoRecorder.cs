using System;
using System.Collections.Generic;
using System.IO;
using Dissonance.Config;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FishsGrandAdventure.Behaviors.Wendigo;

public class WendigoRecorder : MonoBehaviour
{
    private const float folderScanInterval = 8f;
    private const float enemyScanInterval = 5f;

    public static WendigoRecorder Instance;

    private static readonly List<Type> AllowedEnemyTypes = new List<Type>
    {
        typeof(BaboonBirdAI),
        typeof(FlowermanAI),
        typeof(SandSpiderAI),
        typeof(CentipedeAI),
        typeof(SpringManAI),
        typeof(MouthDogAI),
        typeof(DressGirlAI),
        typeof(HoarderBugAI),
        typeof(JesterAI),
        typeof(MaskedPlayerEnemy),
        typeof(NutcrackerEnemyAI),
        typeof(PufferAI),
        typeof(CrawlerAI),
    };

    private static string audioFolder;

    private readonly List<AudioClip> cachedAudio = new List<AudioClip>();

    private float nextTimeToCheckFolder = 30f;
    private float nextTimeToCheckEnemies = 30f;


    private void Awake()
    {
        Instance = this;

        transform.position = Vector3.zero;

        audioFolder = Path.Combine(Application.dataPath, "..", "Dissonance_Diagnostics");
        EnableRecording();

        if (!Directory.Exists(audioFolder))
        {
            Directory.CreateDirectory(audioFolder);
        }
    }

    private void Start()
    {
        try
        {
            if (Directory.Exists(audioFolder))
            {
                Directory.Delete(audioFolder, true);
            }
        }
        catch (Exception message)
        {
            Plugin.Log.LogInfo(message);
        }
    }

    private void Update()
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.Wendigo) return;

        if (Time.realtimeSinceStartup > nextTimeToCheckFolder)
        {
            nextTimeToCheckFolder = Time.realtimeSinceStartup + folderScanInterval;

            if (!Directory.Exists(audioFolder))
            {
                return;
            }

            string[] files = Directory.GetFiles(audioFolder);
            foreach (string path in files)
            {
                Timing.RunCoroutine(LoadWavFile(path,
                        audioClip => { cachedAudio.Add(audioClip); }
                    )
                );
            }
        }

        if (Time.realtimeSinceStartup > nextTimeToCheckEnemies)
        {
            nextTimeToCheckEnemies = Time.realtimeSinceStartup + enemyScanInterval;

            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>(true);
            foreach (EnemyAI enemyAI in enemies)
            {
                if (AllowedEnemyTypes.Contains(enemyAI.GetType()) &&
                    !enemyAI.TryGetComponent(out WendigoMimicry _))
                {
                    enemyAI.gameObject.AddComponent<WendigoMimicry>().Initialize(enemyAI);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        DisableRecording();
    }

    private static void EnableRecording()
    {
        DebugSettings.Instance.EnablePlaybackDiagnostics = true;
        DebugSettings.Instance.RecordFinalAudio = true;
    }

    private static void DisableRecording()
    {
        DebugSettings.Instance.EnablePlaybackDiagnostics = false;
        DebugSettings.Instance.RecordFinalAudio = false;

        if (Directory.Exists(audioFolder))
        {
            Directory.Delete(audioFolder, true);
        }
    }

    private static IEnumerator<float> LoadWavFile(string path, Action<AudioClip> callback)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
        yield return Timing.WaitUntilDone(www.SendWebRequest());

        if (www.result != UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

            if (audioClip.length > 0.9f)
            {
                callback(audioClip);
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(ex);
            }
        }
    }

    public AudioClip GetSample()
    {
        AudioClip result;

        if (cachedAudio.Count > 0)
        {
            int index = Random.Range(0, cachedAudio.Count - 1);
            AudioClip audioClip = cachedAudio[index];
            cachedAudio.RemoveAt(index);
            result = audioClip;
        }
        else
        {
            while (cachedAudio.Count > 200)
            {
                cachedAudio.RemoveAt(0);
            }

            result = null;
        }

        return result;
    }

    public void ClearCache()
    {
        cachedAudio.Clear();
    }
}