using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using Interract;
using Item;

namespace GameUI
{
    public class PlayerUIManager : NetworkBehaviour
    {
        [Header("Weapon Data UI")]
        [SerializeField] TextMeshProUGUI ammoText;
        [SerializeField] Transform weaponHolder;
        [SerializeField] ItemSwitch itemSwitch;


        // Update is called once per frame
        void Update()
        {
            if (!Object.HasInputAuthority)
            {
                transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
                return;
            }
            

            if(weaponHolder.GetChild(itemSwitch.currentSlot).childCount>0)
            {
                weaponHolder.GetChild(itemSwitch.currentSlot).GetChild(0).gameObject.TryGetComponent<WeaponDataMono>(out var weaponData);

                if (weaponData != null && weaponData.isActiveAndEnabled && weaponData.Object != null)
                {
                    int ammoData = weaponData.ammo;
                    ammoText.text = ammoData + "";
                }
                else
                {
                    ammoText.text = "0";
                }
            }
            else
            {
                ammoText.text = "0";
            }
                
        }
    }
}

