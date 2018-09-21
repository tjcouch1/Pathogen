using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSpriteManager : MonoBehaviour {

    [SerializeField] private Image prevWeapon;
    [SerializeField] private Image currentWeapon;
    [SerializeField] private Image nextWeapon;

    [SerializeField] private Animator animator;

    public void setWeapons(PlayerWeapon prev, PlayerWeapon current, PlayerWeapon next, Direction dir)
    {
        if(prev.weaponIcon != prevWeapon.sprite || current.weaponIcon != currentWeapon.sprite || next.weaponIcon != nextWeapon.sprite)
        {
            switchWeapons(prev.weaponIcon, current.weaponIcon, next.weaponIcon, dir);
        }
    }

    private void switchWeapons(Sprite pSprite, Sprite cSprite, Sprite nSprite, Direction dir)
    {
        prevWeapon.sprite = pSprite;
        currentWeapon.sprite = cSprite;
        nextWeapon.sprite = nSprite;

        //TO-DO: Add animation for switching
        //if(dir = switchDirection.up) play up animation
        //else play down animation
    }
}
