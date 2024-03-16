using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class ModifiersControlSystem : NetworkBehaviour
    {
        //List of currently active modifiers and commands waiting for handling
        public List<ActiveModifierData> activeModifiers = new List<ActiveModifierData>();

        public void AddModifier(ModifierBase modifier, ulong senderID, double startTime)
        {
            //Receive modifier and add it to the list of active modifiers
            //It's not modifier handling yet. Only preparation.

            ActiveModifierData container = new ActiveModifierData();
            container.startTime = startTime;
            container.lastUpdateTime = startTime;
            container.ownerID = senderID;
            container.modifier = modifier.UnpackModifier(); //deserialize

            //ContinuousModifier is a base class for modifiers with duration
            if (container.modifier is ContinuousModifier)
            {
                ContinuousModifier continuousModifier = container.modifier as ContinuousModifier;
                container.endTime = startTime + continuousModifier.duration;

                //Check if there is the same modifier. If it is, we suppose to prolong it.
                ActiveModifierData activeModifierWithTheSameTag = GetModifierWithTag<ContinuousModifier>(continuousModifier.tag);

                if (activeModifierWithTheSameTag == null)
                {
                    activeModifiers.Add(container);
                }
                else
                {
                    //Reset activity timer for current modifier (optional)
                    activeModifierWithTheSameTag.startTime = startTime;

                    //Prolong existing modifier
                    activeModifierWithTheSameTag.endTime += continuousModifier.duration;
                }
            }

            //InstantModifier is a base class for modifiers with no duration (commands)
            if (container.modifier is InstantModifier)
            {
                //No default expiration time for commands
                //Suppose to be marked as expired manually, by calling PrepareToRemove() method after handling
                container.endTime = double.MaxValue;

                activeModifiers.Add(container);
            }
        }

        private void FixedUpdate()
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;

            //Remove all expired modifiers
            activeModifiers.RemoveAll(activeModifier => serverTime > activeModifier.endTime);
        }

        public float HandleHealthModifiers(float currentHealth, Action<ActiveModifierData> OnDeath)
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;

            float resultHealth = currentHealth;
            bool isAlive = true;

            //Handle all the modifiers related to health
            for (int i = 0; i < activeModifiers.Count; i++)
            {
                ActiveModifierData container = activeModifiers[i];
                ModifierBase modifier = activeModifiers[i].modifier;

                //Skip if it's too early
                if (container.startTime > serverTime) continue;

                //Check command by type
                if (modifier is InstantHeal)
                {
                    resultHealth += (modifier as InstantHeal).health;

                    //Mark as expired (for instant modifiers only)
                    container.PrepareToRemove();
                }

                //Check command by type
                if (modifier is ContinuousDamage)
                {
                    resultHealth -= (modifier as ContinuousDamage).damage * (float)(serverTime - container.lastUpdateTime);

                    if (resultHealth <= 0 && isAlive == true)
                    {
                        //No luck for this guy. Call death event callback.
                        OnDeath?.Invoke(container);
                        isAlive = false;
                    }
                }

                //Check command by type
                if (modifier is InstantDamage)
                {
                    resultHealth -= (modifier as InstantDamage).damage;

                    if (resultHealth <= 0 && isAlive == true)
                    {
                        //No luck for this guy. Call death event callback.
                        OnDeath?.Invoke(container);
                        isAlive = false;
                    }

                    //Mark as expired (for instant modifiers only)
                    container.PrepareToRemove();
                }
            }

            return resultHealth;
        }

        public float HandleNitroAmountModifiers(float currentNitro)
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;

            float resultNitro = currentNitro;


            //Handle all the modifiers related to health
            for (int i = 0; i < activeModifiers.Count; i++)
            {
                ActiveModifierData container = activeModifiers[i];
                ModifierBase modifier = activeModifiers[i].modifier;

                //Skip if it's too early
                if (container.startTime > serverTime) continue;

                //Check command by type
                if (modifier is InstantNitro)
                {
                    resultNitro += (modifier as InstantNitro).nitroAmount;

                    //Mark as expired (for instant modifiers only)
                    container.PrepareToRemove();
                }
            }

            return resultNitro;
        }

        public int HandleAmmoCountModifiers(WeaponType weaponType, int currentAmmo)
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;

            int resultAmmo = currentAmmo;

            //Handle all the modifiers related to health
            for (int i = 0; i < activeModifiers.Count; i++)
            {
                ActiveModifierData container = activeModifiers[i];
                ModifierBase modifier = activeModifiers[i].modifier;

                //Skip if it's too early
                if (container.startTime > serverTime) continue;

                //Check command by type
                if (modifier is InstantAmmo)
                {
                    InstantAmmo ammoModifier = modifier as InstantAmmo;

                    if (ammoModifier.weapon == weaponType)
                    {
                        //Add ammo
                        resultAmmo += ammoModifier.ammo;

                        //Mark as expired (for instant modifiers only)
                        container.PrepareToRemove();
                    }
                }
            }

            return resultAmmo;
        }

        public ActiveModifierData GetModifier<T>() where T : ModifierBase
        {
            for (int i = 0; i < activeModifiers.Count; i++)
                if (activeModifiers[i].modifier is T) return activeModifiers[i];

            return null;
        }

        public ActiveModifierData GetModifierWithTag<T>(string tag) where T : ModifierBase
        {
            if (tag == string.Empty) return null;

            for (int i = 0; i < activeModifiers.Count; i++)
                if (activeModifiers[i].modifier is T)
                {
                    if (activeModifiers[i].modifier is ContinuousModifier)
                        if ((activeModifiers[i].modifier as ContinuousModifier).tag == tag)
                            return activeModifiers[i];
                }

            return null;
        }


        public void ClearModifiers()
        {
            activeModifiers = new List<ActiveModifierData>();
        }
    }
}
