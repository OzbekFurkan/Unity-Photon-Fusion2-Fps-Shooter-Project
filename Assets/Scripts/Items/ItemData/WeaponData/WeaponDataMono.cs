using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item.Utils;
using System;

namespace Item
{
    
    public class WeaponDataMono : ItemDataMono, IHandAttachable
    {
        [Header("Weapon References")]
        [SerializeField] private ItemReferenceGetter itemReferenceGetter;
        public WeaponDataSettings weaponDataSettings;
        [HideInInspector] public WeaponShootSettings weaponShootSettings;

        //Network
        [Networked, HideInInspector] public int ammo { get; set; }
        [Networked, HideInInspector] public int fullAmmo { get; set; }

        //Hand Attach Points
        public Transform leftHandTarget { get; set; }
        public Transform rightHandTarget { get; set; }

        private void Start()
        {
            leftHandTarget = itemReferenceGetter.LeftHandTargetGetter();
            rightHandTarget = itemReferenceGetter.RightHandTargetGetter();
        }

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

        public Transform GetLeftHandTarget()
        {
            return leftHandTarget;
        }

        public Transform GetRightHandTarget()
        {
            return rightHandTarget;
        }
    }
}

