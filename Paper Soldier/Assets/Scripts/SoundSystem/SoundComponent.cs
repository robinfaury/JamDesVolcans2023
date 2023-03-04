using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SoundComponent
{

    public List<AudioClip> clips = new List<AudioClip> ();
    public RFloat volume = new RFloat();
    public RFloat pitch = new RFloat();
    public float playProbability = 1;

    public SoundComponent () { }

    public SoundComponent(SoundComponent other)
    {
        this.clips = new (other.clips);
        this.volume = new ();
        this.pitch = new ();
    }
}