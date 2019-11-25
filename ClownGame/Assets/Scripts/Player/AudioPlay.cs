using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : Photon.MonoBehaviour
{
    public AudioSource source;
    public float normalPitch;
    public float range;
    public AudioClip[] audioClips;

    void Start()
    {
        normalPitch = source.pitch;
    }

    [PunRPC,HideInInspector]
    public void PlaySound(bool overlay, int index, int maxIndex = 0)
    {
        if(!overlay)
            source.Stop();
        if (maxIndex != 0)
            index = Random.Range(index, maxIndex);
        source.pitch = normalPitch + Random.Range(-range, range);

        source.PlayOneShot(audioClips[index]);
    }
}
