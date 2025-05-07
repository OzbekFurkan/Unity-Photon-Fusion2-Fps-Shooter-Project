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
        [Header("Weapon References")]
        [SerializeField] private ItemReferenceGetter itemReferenceGetter;
        public WeaponDataSettings weaponDataSettings;
        [HideInInspector] public WeaponShootSettings weaponShootSettings => weaponDataSettings.weaponShootSettings;

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
            SetItemDataSettings(weaponDataSettings);

            //we will change these values runtime so we store them seperetly from scriptable object
            ammo = weaponDataSettings.ammo;
            fullAmmo = weaponDataSettings.fullAmmo;
        }

        protected override void SetItemDataSettings(ItemDataSettings itemDataSettings)
        {
            base.itemDataSettings = itemDataSettings;
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

