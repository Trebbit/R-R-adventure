using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class WeaponStatusUIIndicator : MonoBehaviour
    {
        public bool hideOnPC = false;
        public bool hideWhenOutOfAmmo = true;
        public bool showAmmoText = true;

        public Text ammoText;

        public void UpdateAmmo(int ammo, bool isInfinite = false)
        {
            //Hide machinegun indicator/button on standalone platform only
            //We need it on mobile platform as machinegun fire button
            if (hideOnPC == true && Application.isMobilePlatform == false)
            {
                gameObject.SetActive(false);
                return;
            }

            //Hide when out of ammo
            if (ammo == 0 && hideWhenOutOfAmmo == true && isInfinite == false)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (showAmmoText == false) return;

            if (isInfinite == false)
                ammoText.text = ammo.ToString();
            else
                ammoText.text = "";
        }
    }
}