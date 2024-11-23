using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Utilitiy;
using Item;

namespace Player
{
    public class PlayerDataMono : ItemDataMono
    {
        [SerializeField] PlayerDataSettings playerDataSettings;
        [HideInInspector] public byte HP;
        [HideInInspector] public Team team;
        [Networked, HideInInspector] public ref PlayerDataStruct playerData => ref MakeRef<PlayerDataStruct>();

        public void Awake()
        {
            SetBaseProps(playerDataSettings.itemName,
                (int)playerDataSettings.itemId,
                playerDataSettings.itemPrefab,
                playerDataSettings.itemIcon,
                (int)playerDataSettings.itemSlot);
            
            HP = playerDataSettings.HP;
            team = playerDataSettings.team;
        }
        protected override void SetBaseProps(string itemName, int itemId, GameObject itemPrefab, Sprite itemIcon, int itemSlot)
        {
            base.itemName = itemName;
            base.itemId = itemId;
            base.itemPrefab = itemPrefab;
            base.itemIcon = itemIcon;
            base.itemSlot = itemSlot;
        }

        public override void Spawned()
        {
            InitializePlayerDataStruct();
            Debug.Log("username: " + playerData.username);
            Debug.Log("team: " + playerData.team.ToString());
            Debug.Log("kill: " + playerData.kill);
            Debug.Log("death: " + playerData.death);
        }

        private void InitializePlayerDataStruct()
        {
            playerData.playerRef = Object.InputAuthority;
            playerData.username = "Player " + UnityEngine.Random.Range(0, 100);
            playerData.team = playerDataSettings.team;
            playerData.kill = 0;
            playerData.death = 0;
        }
        public string GetUsername()
        {
            return playerData.username.ToString();
        }
        public void AddKill()
        {
            playerData.kill++;
        }
        public void AddDeath()
        {
            playerData.death++;
        }


    }
}

