using UnityEngine;

[System.Serializable]
public class PlayerWeapon {

    public string weaponName = "Default_Weapon";
    public bool isDefault;
    public int damage = 10;
	public int startingClips = 3;
    public int clips;
    public int bullets;
	public bool infiniteAmmo = false;
    public int clipSize = 12;
    public float range = 100f;
	public bool automatic = false;
    public float fireRate = 0f;
    public float reloadTime = 2;
    public float maxInacuracy = 0.02f;
    public float sprayDecayRate = 0.5f;         //Higher values mean faster decay
    public float sprayHealRate = 0.5f;          //Higher values mean faster heal

    public GameObject weaponGFX;
    public Sprite weaponIcon;
    public PlayerAudioClip fireClip;

    public PlayerWeapon()
    {
        weaponSetup();
    }
    
    public void weaponSetup()
    {
        clips = startingClips;
        bullets = clipSize;
    }
}
