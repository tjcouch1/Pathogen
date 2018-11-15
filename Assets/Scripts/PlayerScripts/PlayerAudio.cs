using System.Collections;
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayFootstep()
	{
		PlayerAudioClip step = footstepClips[Random.Range(0, footstepClips.Length)];
		if (step != null)
		{
			footSource.clip = step.clip;
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
		if (landClip != null)
		{
        	footSource.clip = landClip.clip;
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
		if (jumpClip.clip != null)
		{
        	mouthSource.clip = jumpClip.clip;
        	mouthSource.Play();
        }
        else Debug.Log("Jump sound is null!");
    }

	public void PlaySwap()
	{
		if (swapClip != null)
		{
			weaponSource.clip = swapClip.clip;
			weaponSource.Play();
        }
        else Debug.Log("Swap sound is null!");
	}

	public void PlayShoot(PlayerAudioClip fireClip)
	{
        if (fireClip != null)
        {
            weaponSource.clip = fireClip.clip;
            weaponSource.Play();
            Debug.Log("Played fire sound!");
        }
        else Debug.Log("Fire sound is null!");
	}
}
