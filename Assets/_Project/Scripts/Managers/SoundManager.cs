using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundEntry
{
    public string soundName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("SoundManager");
                    instance = obj.AddComponent<SoundManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public SoundEntry[] soundEntries;

    public AudioMixer masterMixer;
    private const string MasterVolumeParam = "MasterVolume";

    private Dictionary<string, AudioClip> soundDict;
    private Dictionary<string, List<AudioSource>> ambientSources =
        new Dictionary<string, List<AudioSource>>();
    private Dictionary<string, AudioSource> uniquePlaying = new Dictionary<string, AudioSource>();
    private readonly Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private Transform poolParent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, AudioClip>();
        foreach (SoundEntry entry in soundEntries)
        {
            if (entry != null && entry.clip != null && !soundDict.ContainsKey(entry.soundName))
            {
                soundDict.Add(entry.soundName, entry.clip);
            }
        }

        GameObject poolGO = new GameObject("AudioSourcePool");
        poolGO.transform.parent = transform;
        poolParent = poolGO.transform;
    }

    public void PlaySound(string[] soundNames, float volume = 1f, float pitch = 1f)
    {
        string randomSound = soundNames[UnityEngine.Random.Range(0, soundNames.Length)];
        PlaySound(randomSound, volume, pitch);
    }

    public void PlaySound(string soundName, float volume = 1f, float pitch = 1f)
    {
        if (!soundDict.ContainsKey(soundName))
        {
            Debug.LogWarning("Sound not found: " + soundName);
            return;
        }

        AudioClip clip = soundDict[soundName];
        AudioSource aSource = GetPooledAudioSource();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.pitch = pitch;
        aSource.loop = false;
        aSource.Play();
        ReturnAudioSourceWhenFinished(aSource, clip.length / pitch).Forget();
    }

    public void PlayUnique(string[] soundNames, float volume = 1f, float pitch = 1f)
    {
        string randomSound = soundNames[UnityEngine.Random.Range(0, soundNames.Length)];
        PlayUnique(randomSound, volume, pitch);
    }

    public void PlayUnique(string soundName, float volume = 1f, float pitch = 1f)
    {
        if (uniquePlaying.ContainsKey(soundName))
        {
            return;
        }
        if (!soundDict.ContainsKey(soundName))
        {
            Debug.LogWarning("Sound not found: " + soundName);
            return;
        }

        AudioClip clip = soundDict[soundName];
        GameObject uniqueGO = new GameObject("UniqueAudio_" + soundName);
        uniqueGO.transform.parent = transform;
        AudioSource aSource = uniqueGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.pitch = pitch;
        aSource.loop = false;
        aSource.Play();

        uniquePlaying.Add(soundName, aSource);
        RemoveUniqueWhenFinished(soundName, aSource, clip.length / pitch).Forget();
    }

    public void PlayAmbientSong(string songName, float volume = 1f)
    {
        if (!soundDict.ContainsKey(songName))
        {
            Debug.LogWarning("Ambient song not found: " + songName);
            return;
        }

        AudioClip clip = soundDict[songName];
        GameObject ambientGO = new GameObject("AmbientAudio_" + songName);
        ambientGO.transform.parent = transform;
        AudioSource aSource = ambientGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.loop = true;
        aSource.Play();

        // Add to the ambient sources list.
        if (!ambientSources.ContainsKey(songName))
        {
            ambientSources[songName] = new List<AudioSource>();
        }
        ambientSources[songName].Add(aSource);
    }

    public void PlayUniqueAmbientSong(string songName, float volume = 1f)
    {
        // Check if any ambient source for this song is already playing.
        if (ambientSources.ContainsKey(songName) && ambientSources[songName].Count > 0)
        {
            return;
        }
        PlayAmbientSong(songName, volume);
    }

    public void PlayUniqueAmbientSong(string[] songNames, float volume = 1f)
    {
        string randomSong = songNames[UnityEngine.Random.Range(0, songNames.Length)];
        PlayUniqueAmbientSong(randomSong, volume);
    }

    public void StopAmbientSong(string songName = "")
    {
        if (string.IsNullOrEmpty(songName))
        {
            // Stop all ambient songs.
            foreach (var kvp in ambientSources)
            {
                foreach (AudioSource src in kvp.Value)
                {
                    if (src != null)
                    {
                        src.Stop();
                        Destroy(src.gameObject);
                    }
                }
            }
            ambientSources.Clear();
        }
        else if (ambientSources.ContainsKey(songName))
        {
            foreach (AudioSource src in ambientSources[songName])
            {
                if (src != null)
                {
                    src.Stop();
                    Destroy(src.gameObject);
                }
            }
            ambientSources.Remove(songName);
        }
    }

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        else
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.parent = poolParent;
            AudioSource source = go.AddComponent<AudioSource>();
            if (masterMixer != null)
            {
                AudioMixerGroup[] groups = masterMixer.FindMatchingGroups("Master");
                if (groups.Length > 0)
                    source.outputAudioMixerGroup = groups[0];
            }
            return source;
        }
    }

    private async UniTaskVoid ReturnAudioSourceWhenFinished(AudioSource aSource, float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        aSource.Stop();
        aSource.clip = null;
        aSource.gameObject.SetActive(false);
        audioSourcePool.Enqueue(aSource);
    }

    private async UniTaskVoid RemoveUniqueWhenFinished(
        string soundName,
        AudioSource aSource,
        float delay
    )
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        if (uniquePlaying.ContainsKey(soundName))
        {
            uniquePlaying.Remove(soundName);
        }
        if (aSource != null)
        {
            Destroy(aSource.gameObject);
        }
    }

    public void SetMute(bool mute)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat(MasterVolumeParam, mute ? -80f : 0f);
        }
        else
        {
            AudioListener.volume = mute ? 0f : 1f;
        }
    }
}
