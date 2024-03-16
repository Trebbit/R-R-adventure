using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class StatusBarUIController : MonoBehaviour
    {
        public Text userNameTextComponent;

        [Space]
        public Slider healthStatusSlider;
        public Slider nitroStatusSlider;

        [Space]
        public WeaponStatusUIIndicator rocketLauncherIndicator;
        public WeaponStatusUIIndicator mineLauncherIndicator;

        [Space]
        public float verticalOffset = 1.5f;

        private Transform mainCamera;

        private void Awake()
        {
            healthStatusSlider.value = healthStatusSlider.maxValue;
            nitroStatusSlider.value = nitroStatusSlider.maxValue;
            transform.localScale = Vector3.one;

            mainCamera = Camera.main.transform;
        }
        public void ShowUserName(string userName)
        {
            if (userNameTextComponent != null)
            {
                userNameTextComponent.gameObject.SetActive(true);
                userNameTextComponent.text = userName;
            }
        }

        public void UpdateHealthAmount(float currentHP, float maxHP)
        {
            healthStatusSlider.value = currentHP / maxHP;
        }

        public void UpdateNitroAmount(float currentNitro, float maxNitro)
        {
            nitroStatusSlider.value = currentNitro / maxNitro;
        }

        public void UpdateStatusBarPosition(Vector3 pivotPoint)
        {
            //Place above car
            transform.position = pivotPoint + Vector3.up * verticalOffset;

            //Look towards the camera
            Vector3 lookDirection = transform.position - mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }

        public void UpdateWeaponIndicators(List<Weapon> weapons)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
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