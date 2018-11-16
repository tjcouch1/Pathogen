using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAudioClip
{
    public enum FalloffDistances
    {
        VeryClose = 200,
        Close = 350,
        Medium = 500,
        Far = 4000,
        VeryFar = 10000
    }

    public AudioClip clip;
    public FalloffDistances maxDistance = FalloffDistances.Medium;
}
