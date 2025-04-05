using Unity.VisualScripting;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public string soundName;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0f, 2f)]
    public float pitch = 1f;

    [Range(0f, 2f)]
    public float pitch2 = 0f;

    public void Play()
    {
        if (SoundManager.Instance != null)
        {
            var p = pitch2 == 0f ? pitch : UnityEngine.Random.Range(pitch2, pitch);
            SoundManager.Instance.PlaySound(soundName, volume, p);
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found.");
        }
    }

    public void PlayUnique()
    {
        if (SoundManager.Instance != null)
        {
            var p = pitch2 == 0f ? pitch : UnityEngine.Random.Range(pitch2, pitch);
            SoundManager.Instance.PlayUnique(soundName, volume, p);
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found.");
        }
    }
}
