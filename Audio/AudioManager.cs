using System.Collections.Generic;
using System.IO;
using FishsGrandAdventure.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace FishsGrandAdventure.Audio;

[PublicAPI]
public class AudioManager : MonoBehaviour
{
    public static readonly Dictionary<string, AudioClip> LoadedMusic = new Dictionary<string, AudioClip>();
    public static readonly Dictionary<string, AudioClip> LoadedSFX = new Dictionary<string, AudioClip>();
    public static AudioSource MusicSource;
    public static AudioSource SFXSource;

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

        SFXSource = gameObject.AddComponent<AudioSource>();
        SFXSource.bypassReverbZones = true;
        SFXSource.bypassListenerEffects = true;
        SFXSource.bypassEffects = true;
        SFXSource.reverbZoneMix = 0;
        SFXSource.spatialBlend = 0;
        SFXSource.spatialize = false;
        SFXSource.spatializePostEffects = false;

        Timing.RunCoroutine(LoadAudioResources());

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene prev, Scene current)
    {
        MusicSource.Stop();
        SFXSource.Stop();
    }

    public static void PlayMusic(string name, float volume = .85f, float pitch = 1f, bool loop = false)
    {
        if (!LoadedMusic.ContainsKey(name))
        {
            Debug.LogError($"AudioManager: Could not find audio clip with name {name}");
            return;
        }

        MusicSource.Stop();

        MusicSource.clip = LoadedMusic[name];
        MusicSource.volume = volume;
        MusicSource.pitch = pitch;
        MusicSource.loop = loop;

        MusicSource.Play();
    }

    public static void PlaySFXAtPoint(string name, Vector3 pos, float volume = .85f)
    {
        if (!LoadedSFX.ContainsKey(name))
        {
            Debug.LogError($"AudioManager: Could not find audio clip with name {name}");
            return;
        }

        AudioSource.PlayClipAtPoint(LoadedSFX[name], pos, volume);
    }

    public static void PlaySFX(string name, float volume = .85f)
    {
        if (!LoadedSFX.ContainsKey(name))
        {
            Debug.LogError($"AudioManager: Could not find audio clip with name {name}");
            return;
        }

        SFXSource.PlayOneShot(LoadedSFX[name], volume);
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
            Debug.Log($"Loading audio Music file: {Path.GetFileName(file)}");

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
                LoadedMusic[Path.GetFileNameWithoutExtension(web.url)] = audioClip;
            }
        }

        foreach (string file in Directory.GetFiles($"{Plugin.FileLocation}/../SFX"))
        {
            Debug.Log($"Loading audio SFX file: {Path.GetFileName(file)}");

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
                LoadedSFX[Path.GetFileNameWithoutExtension(web.url)] = audioClip;
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