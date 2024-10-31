using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Utilitiy;

namespace Interract
{
    public class PlayerDataMono : ItemDataMono
    {
        [SerializeField] PlayerDataSettings playerDataSettings;
        [HideInInspector] public Team team;
        [HideInInspector] public byte HP;
        

        public void Awake()
        {
            SetBaseProps((int)playerDataSettings.itemId,
                playerDataSettings.itemPrefab,
                playerDataSettings.itemIcon,
                (int)playerDataSettings.itemSlot);

            team = playerDataSettings.team;
            HP = playerDataSettings.HP;
        }

        protected override void SetBaseProps(int itemId, GameObject itemPrefab, Sprite itemIcon, int itemSlot)
        {
            base.itemId = itemId;
            base.itemPrefab = itemPrefab;
            base.itemIcon = itemIcon;
            base.itemSlot = itemSlot;
        }
    }
}

