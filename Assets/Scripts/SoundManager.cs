using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource hitAudio;
    public AudioSource WaitAudio;

    public void PlayHitAudio()
    {
        hitAudio.Play();
    }
    public void PlayWaitAudio()
    {
        WaitAudio.Play();
    }
}
