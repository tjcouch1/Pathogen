using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 11, sendInterval = 0.1f)]
public class PlayerAudio : NetworkBehaviour {

	public AudioSource footSource;
	public PlayerAudioClip[] footstepClips;
	public PlayerAudioClip landClip;
    public PlayerAudioClip hurtSound;
    public PlayerAudioClip deathSound;

	public AudioSource mouthSource;
	public PlayerAudioClip jumpClip;

	public AudioSource weaponSource;
	public PlayerAudioClip swapClip;
    public PlayerAudioClip reloadClip;

	private Coroutine footstepCoroutine;
	private float footstepSpeed = .3125f;

    [SerializeField] private float volume = .4f;

    void Start()
    {
        footSource.volume = volume;
        mouthSource.volume = volume;
        weaponSource.volume = volume;
    }

    //send play sound to server, server sends to clients
    [Command]
    public void CmdStartPlayFootsteps()
    {
        RpcStartPlayFootsteps();
    }

    //start playing sound on clients other than local player
    [ClientRpc]
    public void RpcStartPlayFootsteps()
    {
        if (!isLocalPlayer)
		{
            StartPlayFootsteps();
		}
    }

	public void StartPlayFootsteps()
	{
		footstepCoroutine = StartCoroutine(PlayFootsteps());
    }

    //send stop footsteps to server, server sends to clients
    [Command]
    public void CmdStopPlayFootsteps()
    {
        RpcStopPlayFootsteps();
    }

    //stop playing sound on clients other than local player
    [ClientRpc]
    public void RpcStopPlayFootsteps()
    {
        if (!isLocalPlayer)
            StopPlayFootsteps();
    }

    //stop playing sound on all clients
    [ClientRpc]
    public void RpcStopPlayAllFootsteps()
    {
        StopPlayFootsteps();
    }

	public void StopPlayFootsteps()
	{
        if (footstepCoroutine != null)
		{
            StopCoroutine(footstepCoroutine);
        	footstepCoroutine = null;
        }
	}

	public IEnumerator PlayFootsteps()
    {
        while (true)
        {
            PlayFootstep();
            yield return new WaitForSeconds(footstepSpeed);
        }

	}

	public void PlayFootstep()
	{
		PlayerAudioClip step = footstepClips[Random.Range(0, footstepClips.Length)];
		if (step != null && step.clip != null)
		{
			footSource.clip = step.clip;
			footSource.maxDistance = step.getMaxDistance();
			footSource.Play();
		}
		else Debug.Log("Footstep sound is null!");
    }

    public void PlayLand()
    {
		if (landClip != null && landClip.clip != null)
		{
        	footSource.clip = landClip.clip;
            footSource.maxDistance = landClip.getMaxDistance();
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
            mouthSource.maxDistance = jumpClip.getMaxDistance();
        	mouthSource.Play();
        }
        else Debug.Log("Jump sound is null!");
    }

	public void PlaySwap()
	{
		if (swapClip != null && swapClip.clip != null)
		{
			weaponSource.clip = swapClip.clip;
            weaponSource.maxDistance = swapClip.getMaxDistance();
			weaponSource.Play();
        }
        else Debug.Log("Swap sound is null!");
	}

	public void PlayShoot(PlayerAudioClip fireClip)
	{
        if (fireClip != null && fireClip.clip != null)
        {
            weaponSource.clip = fireClip.clip;
            weaponSource.maxDistance = fireClip.getMaxDistance();
            weaponSource.Play();
        }
        else Debug.Log("Fire sound is null!");
	}

    public void PlayReload()
    {
        if (reloadClip != null && reloadClip.clip != null)
        {
            weaponSource.clip = reloadClip.clip;
            weaponSource.maxDistance = reloadClip.getMaxDistance();
            weaponSource.Play();
        }
        else Debug.Log("Reload sound is null!");
    }

    public void PlayHurtAudio()
    {
        if (hurtSound != null && hurtSound.clip != null)
        {
            footSource.clip = hurtSound.clip;
            footSource.maxDistance = hurtSound.getMaxDistance();
            footSource.Play();
        }
        else Debug.Log("Reload sound is null!");
    }

    public void PlayDeathAudio()
    {
        if (deathSound != null && deathSound.clip != null)
        {
            footSource.clip = deathSound.clip;
            footSource.maxDistance = deathSound.getMaxDistance();
            footSource.Play();
        }
        else Debug.Log("Reload sound is null!");
    }

	void OnDestroy()
	{
		StopPlayFootsteps();
	}
}
