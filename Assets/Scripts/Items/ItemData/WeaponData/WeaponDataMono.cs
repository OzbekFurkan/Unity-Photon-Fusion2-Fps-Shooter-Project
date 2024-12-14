using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item;
using System;

namespace Item
{
    
    public class WeaponDataMono : ItemDataMono, IHandAttachable
    {
        [Header("Weapon Data Scriptable Objects")]
        [SerializeField] public WeaponDataSettings weaponDataSettings;
        [HideInInspector] public WeaponShootSettings weaponShootSettings;

        //Network
        [Networked, HideInInspector] public int ammo { get; set; }
        [Networked, HideInInspector] public int fullAmmo { get; set; }

        [Header("Hand Attach")]
        public Transform leftHandTransform;
        public Transform rightHandTransform;

        public override void Spawned()
        {

            SetBaseProps(weaponDataSettings.itemName,
                (int)weaponDataSettings.itemId,
                weaponDataSettings.itemPrefab,
                weaponDataSettings.itemIcon,
                (int)weaponDataSettings.itemSlot);

            ammo = weaponDataSettings.ammo;
            fullAmmo = weaponDataSettings.fullAmmo;
            weaponShootSettings = weaponDataSettings.weaponShootSettings;
        }

        protected override void SetBaseProps(string itemName, int itemId, GameObject itemPrefab, Sprite itemIcon, int itemSlot)
        {
            base.itemName = itemName;
            base.itemId = itemId;
            base.itemPrefab = itemPrefab;
            base.itemIcon = itemIcon;
            base.itemSlot = itemSlot;
        }

        public Transform GetLeftHandTransform()
        {
            return leftHandTransform;
        }

        public Transform GetRightHandTransform()
        {
            return rightHandTransform;
        }
    }
}

