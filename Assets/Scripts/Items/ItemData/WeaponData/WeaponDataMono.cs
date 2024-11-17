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

        [Header("Network")]
        private ChangeDetector changeDetector;
        [Networked]
        [HideInInspector] public int ammo { get; set; }
        [HideInInspector] public int fullAmmo;

        [Header("Hand Attach")]
        public Transform leftHandTransform;
        public Transform rightHandTransform;

        public override void Spawned()
        {
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            SetBaseProps(weaponDataSettings.itemName,
                (int)weaponDataSettings.itemId,
                weaponDataSettings.itemPrefab,
                weaponDataSettings.itemIcon,
                (int)weaponDataSettings.itemSlot);

            ammo = weaponDataSettings.ammo;
            fullAmmo = weaponDataSettings.fullAmmo;
            weaponShootSettings = weaponDataSettings.weaponShootSettings;
        }

        public override void Render()
        {
            foreach (var change in changeDetector.DetectChanges(this, out var previous, out var current))
            {
                switch (change)
                {
                    case nameof(ammo):
                        var reader = GetPropertyReader<int>(nameof(ammo));
                        int receivedAmmo = reader.Read(current);
                        OnAmmoChanged(receivedAmmo);
                        break;
                }
            }
        }

        public void OnAmmoChanged(int newAmmo)
        {
            ammo = newAmmo;
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

