using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class HUDController : MonoBehaviour
    {
        public Slider healthStatusSlider;
        public Slider nitroStatusSlider;

        [Space]
        public Text lapTextComponent;
        public Text positionTextComponent;
        public Text speedTextComponent;

        [Space]
        public WeaponStatusUIIndicator machinegunIndicator;
        public WeaponStatusUIIndicator rocketLauncherIndicator;
        public WeaponStatusUIIndicator mineLauncherIndicator;

        private void Awake()
        {
            healthStatusSlider.value = healthStatusSlider.maxValue;
            nitroStatusSlider.value = nitroStatusSlider.maxValue;
        }

        public void UpdateHealthAmount(float currentHP, float maxHP)
        {
            healthStatusSlider.value = currentHP / maxHP;
        }

        public void UpdateNitroAmount(float currentNitro, float maxNitro)
        {
            nitroStatusSlider.value = currentNitro / maxNitro;
        }

        public void UpdateSpeed(float currentSpeed)
        {
            if (speedTextComponent != null) speedTextComponent.text = currentSpeed.ToString("N0") + " km/h";
        }

        public void UpdateRaceStatus(int currentLap, int maxLaps, int currentPlace)
        {
            //Update position text
            positionTextComponent.text = "Position: " + currentPlace;

            //Update lap text
            lapTextComponent.text = "Lap: " + Math.Clamp((currentLap + 1), 1, maxLaps) + "/" + maxLaps;

            //Before car crossed start line 
            if (currentPlace == -1)
                positionTextComponent.text = "Position: --";

            //Before car crossed start line 
            if (currentLap == -1)
                lapTextComponent.text = "Lap: --";
        }

        public void UpdateWeaponIndicators(List<Weapon> weapons)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].weaponType == WeaponType.MachineGun)
                {
                    //Machinegun
                    machinegunIndicator.UpdateAmmo(weapons[i].ammo, weapons[i].isInfiniteAmmo);
                }

                if (weapons[i].weaponType == WeaponType.RocketLauncher)
                {
                    //Missile
                    rocketLauncherIndicator.UpdateAmmo(weapons[i].ammo, weapons[i].isInfiniteAmmo);
                }

                if (weapons[i].weaponType == WeaponType.MineLauncher)
                {
                    //Mine
                    mineLauncherIndicator.UpdateAmmo(weapons[i].ammo, weapons[i].isInfiniteAmmo);
                }
            }
        }
    }
}