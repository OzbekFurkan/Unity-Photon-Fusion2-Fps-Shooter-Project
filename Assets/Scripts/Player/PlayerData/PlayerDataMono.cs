using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Utilitiy;
using Item;

namespace Player
{
    public enum PlayerState { Playing, Interacting, Reloading, Paused, Died}

    public class PlayerDataMono : ItemDataMono
    {
        //scriptable object that hold various player data to be read
        [SerializeField] PlayerDataSettings playerDataSettings;

        //player props to be assigned by player data settings (scriptable object)
        [HideInInspector] public byte HP;
        [HideInInspector] public Team team;

        //needed for ui
        [Networked, HideInInspector] public ref PlayerDataStruct playerData => ref MakeRef<PlayerDataStruct>();

        //player state
        [HideInInspector] public PlayerState playerState = PlayerState.Playing;
        //player state stack to store recent states
        [HideInInspector] public StateStack<PlayerState> playerStateStack = new StateStack<PlayerState>();

        public void Awake()
        {
            SetBaseProps(playerDataSettings.itemName,
                (int)playerDataSettings.itemId,
                playerDataSettings.itemPrefab,
                playerDataSettings.itemIcon,
                (int)playerDataSettings.itemSlot);
            
            HP = playerDataSettings.HP;
            team = playerDataSettings.team;
            playerStateStack.Add(playerState);
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
            if (!Object.HasInputAuthority)
                return;

            InitializePlayerDataStructRpc(PlayerPrefs.GetString("username"));
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

