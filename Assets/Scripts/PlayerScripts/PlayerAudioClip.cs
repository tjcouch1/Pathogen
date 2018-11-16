using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAudioClip
{
    public enum FalloffValues
    {
        VeryClose,
        Close,
        Medium,
        Far,
        VeryFar
    }

    public int getMaxDistance()
    {
        switch (maxDistance)
        {
            case FalloffValues.VeryClose:
                return 7;
            case FalloffValues.Close:
                return 15;
            case FalloffValues.Medium:
                return 30;
            case FalloffValues.Far:
                return 50;
            case FalloffValues.VeryFar:
                return 100;

        }
        Debug.Log("Somehow got out of getMaxDistance");
        return 0;
    }

    public AudioClip clip;
    [SerializeField] private FalloffValues maxDistance = FalloffValues.Medium;
}
