using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using Interract;
using Item;
using UnityEngine.UI;

namespace GameUI
{
    public class PlayerUIManager : NetworkBehaviour
    {
        [Header("PlayerData")]
        [SerializeField] PlayerDataMono playerDataMono;
        [SerializeField] GameObject playerUi;
        [SerializeField] ItemSwitch itemSwitch;
        [SerializeField] Transform weaponHolder;

        [Header("Weapon Data UI")]
        [SerializeField] TextMeshProUGUI ammoText; 

        [Header("Hp Data UI")]
        [SerializeField] Slider hpBarSlider;

        [Header("SlotUI")]
        [SerializeField] GameObject slotUI;
        [SerializeField] Sprite blankSlotIcon;

        // Update is called once per frame
        void Update()
        {
            if (!Object.HasInputAuthority)
            {
                playerUi.SetActive(false);
                return;
            }

            SetAmmoText();
            SetHpBarSlider();
            SetSlotUI();
               
        }

        private void SetAmmoText()
        {
            if (weaponHolder.GetChild(itemSwitch.currentSlot).childCount > 0)
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
        private void SetHpBarSlider()
        {
            hpBarSlider.value = playerDataMono.HP;
        }
        private void SetSlotUI()
        {
            //SetAllIcons
            for (int i = 0; i < weaponHolder.childCount - 1/*slot amount, 'Other' excluded by '-1'*/; i++)
            {
                //empty slots setted as blank icon
                if (weaponHolder.GetChild(i).childCount <= 0)
                {
                    slotUI.transform.GetChild(i).gameObject.TryGetComponent<Image>(out Image image);
                    if (image != null)
                    {
                        image.sprite = blankSlotIcon;
                        //dark-gray overlay by default
                        image.color = SetOverlayOnSlotIcons("#858585");
                    }
                }
                //non-empty slots setted as their own icons
                else
                {
                    GameObject item = weaponHolder.GetChild(i).GetChild(0).gameObject;
                    item.TryGetComponent<ItemDataMono>(out ItemDataMono itemData);
                    if(itemData != null)
                    {
                        slotUI.transform.GetChild(i).gameObject.TryGetComponent<Image>(out Image image);
                        if(image != null)
                        {
                            image.sprite = itemData.itemIcon;
                            //dark-gray overlay by default
                            image.color = SetOverlayOnSlotIcons("#858585");
                        }
                    }
                }

            }

            //Set Current Slot Overlay White
            slotUI.transform.GetChild(itemSwitch.currentSlot).gameObject.TryGetComponent<Image>(out Image currentSlotImage);
            if(currentSlotImage != null)
            {
                currentSlotImage.color = SetOverlayOnSlotIcons("#FFFFF");
            }

        }
        private Color SetOverlayOnSlotIcons(string htmlValue)
        {
            Color convertedColor = Color.white;
            ColorUtility.TryParseHtmlString(htmlValue, out convertedColor);
            return convertedColor;
        }


    }
}

