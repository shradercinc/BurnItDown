using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    AudioSource source;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        source = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip sound)
    {
        source.PlayOneShot(sound);
    }

    public void PlaySound(AudioClip sound, Vector3 position)
    {
        transform.position = position;
        source.PlayOneShot(sound);
    }

    public void PlaySoundDelayed(AudioClip sound, float delay)
    {
        StartCoroutine(SoundDelayCoroutine(sound, delay));
    }

    IEnumerator SoundDelayCoroutine(AudioClip sound, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(sound);
    }
}
