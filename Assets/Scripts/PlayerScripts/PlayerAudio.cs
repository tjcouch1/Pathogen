﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 11, sendInterval = 0.1f)]
public class PlayerAudio : NetworkBehaviour {

	public AudioSource footSource;
	public PlayerAudioClip[] footstepClips;
	public PlayerAudioClip landClip;

	public AudioSource mouthSource;
	public PlayerAudioClip jumpClip;

	public AudioSource weaponSource;
	public PlayerAudioClip swapClip;

	public void PlayFootstep()
	{
		PlayerAudioClip step = footstepClips[Random.Range(0, footstepClips.Length)];
		if (step != null && step.clip != null)
		{
			footSource.clip = step.clip;
			footSource.maxDistance = (int) step.maxDistance;
			footSource.Play();
            Debug.Log("Played footstep sound!");
		}
		else Debug.Log("Footstep sound is null!");
    }

    //send play sound to server, server sends to clients
    [Command]
    public void CmdPlayLand()
    {
        RpcPlayLand();
    }

    //play sound on clients other than local player
    [ClientRpc]
    public void RpcPlayLand()
    {
        if (!isLocalPlayer)
            PlayLand();
    }

    public void PlayLand()
    {
		if (landClip != null && landClip.clip != null)
		{
        	footSource.clip = landClip.clip;
            footSource.maxDistance = (int) landClip.maxDistance;
        	footSource.Play();
        }
        else Debug.Log("Land sound is null!");
    }

	//send play sound to server, server sends to clients
	[Command]
	public void CmdPlayJump()
	{
		RpcPlayJump();
	}

    //play sound on clients other than local player
    [ClientRpc]
	public void RpcPlayJump()
	{
		if (!isLocalPlayer)
			PlayJump();
	}

    //play sound! Runs directly on clients
    public void PlayJump()
    {
		if (jumpClip != null && jumpClip.clip != null)
		{
        	mouthSource.clip = jumpClip.clip;
            mouthSource.maxDistance = (int) jumpClip.maxDistance;
        	mouthSource.Play();
        }
        else Debug.Log("Jump sound is null!");
    }

	public void PlaySwap()
	{
		if (swapClip != null && swapClip.clip != null)
		{
			weaponSource.clip = swapClip.clip;
            weaponSource.maxDistance = (int) swapClip.maxDistance;
			weaponSource.Play();
        }
        else Debug.Log("Swap sound is null!");
	}

	public void PlayShoot(PlayerAudioClip fireClip)
	{
        if (fireClip != null && fireClip.clip != null)
        {
            weaponSource.clip = fireClip.clip;
            weaponSource.maxDistance = (int) fireClip.maxDistance;
            weaponSource.Play();
            Debug.Log("Played fire sound!");
        }
        else Debug.Log("Fire sound is null!");
	}
}
