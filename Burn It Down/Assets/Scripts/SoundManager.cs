using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    AudioSource source;
    public AudioClip stun, demolish;

    AudioClip scheduledSound;
    
    void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip sound)
    {
        source.PlayOneShot(sound);
    }

    public void PlaySound(AudioClip sound, float t)
    {
        scheduledSound = sound;
        StartCoroutine("WaitToPlaySound", t);
    }

    IEnumerator WaitToPlaySound(float t)
    {
        yield return new WaitForSeconds(t);
        PlaySound(scheduledSound);
    }
}
