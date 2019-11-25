using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleSoundsStart : MonoBehaviour
{
    public AudioClip[] clips;

    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        foreach (AudioClip clip in clips)
            source.PlayOneShot(clip);
    }
}
