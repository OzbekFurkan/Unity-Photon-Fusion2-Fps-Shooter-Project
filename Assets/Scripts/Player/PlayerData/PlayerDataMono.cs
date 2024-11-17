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
        [Networked, HideInInspector] public ref PlayerDataStruct playerData => ref MakeRef<PlayerDataStruct>();

        public void Awake()
        {
            SetBaseProps(playerDataSettings.itemName,
                (int)playerDataSettings.itemId,
                playerDataSettings.itemPrefab,
                playerDataSettings.itemIcon,
                (int)playerDataSettings.itemSlot);
            
            HP = playerDataSettings.HP;
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
            playerData.team = Team.Blue;
            playerData.kill = 0;
            playerData.death = 0;
        }
        public void UpdateUsername(string username)
        {
            playerData.username = username;
        }
        public string GetUsername()
        {
            return playerData.username.ToString();
        }
        public void UpdateTeam(Team team)
        {
            playerData.team = team;
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

