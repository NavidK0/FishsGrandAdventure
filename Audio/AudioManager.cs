using System.Collections.Generic;
using System.IO;
using FishsGrandAdventure.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace FishsGrandAdventure.Audio;

[PublicAPI]
public class AudioManager : MonoBehaviour
{
    public static readonly Dictionary<string, AudioClip> LoadedAudio = new Dictionary<string, AudioClip>();
    public static AudioSource MusicSource;

    private void Awake()
    {
        MusicSource = gameObject.AddComponent<AudioSource>();
        MusicSource.bypassReverbZones = true;
        MusicSource.bypassListenerEffects = true;
        MusicSource.bypassEffects = true;
        MusicSource.reverbZoneMix = 0;
        MusicSource.spatialBlend = 0;
        MusicSource.spatialize = false;
        MusicSource.spatializePostEffects = false;

        Timing.RunCoroutine(LoadAudioResources());
    }

    public static void PlayMusic(string name, float volume = .85f, float pitch = 1f, bool loop = false)
    {
        if (!LoadedAudio.ContainsKey(name))
        {
            Debug.LogError($"AudioManager: Could not find audio clip with name {name}");
            return;
        }

        MusicSource.Stop();

        MusicSource.clip = LoadedAudio[name];
        MusicSource.volume = volume;
        MusicSource.pitch = pitch;
        MusicSource.loop = loop;

        MusicSource.Play();
    }

    public static void StopMusic(bool fadeOut = false, float fadeOutDuration = 1f)
    {
        if (fadeOut)
        {
            Timing.RunCoroutine(FadeOutMusic(fadeOutDuration));
        }
        else
        {
            MusicSource.Stop();
        }
    }

    private static IEnumerator<float> FadeOutMusic(float packetFadeOutDuration)
    {
        float startVolume = MusicSource.volume;

        while (MusicSource.volume > 0)
        {
            MusicSource.volume -= Time.deltaTime / packetFadeOutDuration;

            yield return Timing.WaitForOneFrame;
        }

        MusicSource.Stop();
        MusicSource.volume = startVolume;
    }

    private IEnumerator<float> LoadAudioResources()
    {
        Debug.Log("Loading audio resources...");

        foreach (string file in Directory.GetFiles($"{Plugin.FileLocation}/../Music"))
        {
            Debug.Log($"Loading audio file: {Path.GetFileName(file)}");

            using UnityWebRequest web =
                UnityWebRequestMultimedia.GetAudioClip($"file://{file}",
                    Path.GetExtension(file) == ".mp3" ? AudioType.MPEG : AudioType.OGGVORBIS
                );

            yield return Timing.WaitUntilDone(web.SendWebRequest());

            if (web.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(web.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(web);
                LoadedAudio[Path.GetFileNameWithoutExtension(web.url)] = audioClip;
            }
        }
    }

    private AudioType GetAudioType(string extension)
    {
        switch (extension)
        {
            case ".mp2":
            case ".mp3":
                return AudioType.MPEG;
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".wav":
                return AudioType.WAV;
            case ".aiff":
                return AudioType.AIFF;
            default: return AudioType.UNKNOWN;
        }
    }
}