using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Utilitiy;
using Item;

namespace Player
{
    enum PlayerState { Available, Interracting, Reloading, Paused}
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

        #region PLAYER_DATA_STRUCT_ACTIONS
        public override void Spawned()
        {
            if(Object.HasInputAuthority)
                InitializePlayerDataStructRpc(PlayerPrefs.GetString("username"));

            Debug.Log("username: " + playerData.username);
            Debug.Log("team: " + playerData.team.ToString());
            Debug.Log("kill: " + playerData.kill);
            Debug.Log("death: " + playerData.death);
        }

        [Rpc(sources:RpcSources.InputAuthority, targets:RpcTargets.StateAuthority)]
        private void InitializePlayerDataStructRpc(string username)
        {
            playerData.playerRef = Object.InputAuthority;
            playerData.username = username;
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
        #endregion

    }
}

