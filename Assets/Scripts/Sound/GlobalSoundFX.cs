using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class allows a GameObject to play a sound and destroy after completion.
 */
public class GlobalSoundFX : MonoBehaviour
{
    public AudioClip soundToPlay;
    AudioSource source;

    public GlobalSoundFX(AudioClip sound)
    {
        this.soundToPlay = sound;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void playGlobalSound(AudioClip sound, float volume)
    {
        soundToPlay = sound;
        source = GetComponentInParent<AudioSource>();
        source.PlayOneShot(soundToPlay, volume);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (soundToPlay != null && !source.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
