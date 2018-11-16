using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAudioClip
{
    public enum FalloffDistances
    {
        VeryClose = 100,
        Close = 250,
        Medium = 500,
        Far = 1000,
        VeryFar = 2000
    }

    public AudioClip clip;
    public FalloffDistances maxDistance = FalloffDistances.Medium;
}
