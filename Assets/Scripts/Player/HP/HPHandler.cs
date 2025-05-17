using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.UI;
using GameModes.Common;
using GameModes.Modes;
using Player.Inventory;

namespace Player
{
    public class HPHandler : NetworkBehaviour
    {
        [Header("Player References")]
        [SerializeField] PlayerDataMono playerDataMono;
        [SerializeField] ItemSwitch itemSwitch;
        [SerializeField] SimpleKCC KCC;
        [SerializeField] InventoryManager inventoryManager;
        [SerializeField] Transform weaponHolder;
        [SerializeField] HitboxRoot hitboxRoot;
        [SerializeField] CharacterMovementHandler characterMovementHandler;

        //Network Related Variables
        [Networked, OnChangedRender(nameof(OnStateChanged))] public bool isDead { get; set; } = false;
        [Networked] public byte HP { get; set; }//runtime hp value

        [Header("Effects")]
        public GameObject playerModel;//our model will be invisible when died and will be visible when revieved
        public GameObject deathGameObjectPrefab;//particle effect to play when died

        //teleport position when player died
        private Vector3? pendingTeleportPosition;

        public override void Spawned()
        {
            //set starting hp
            HP = playerDataMono.startingHP;
        }

        public override void FixedUpdateNetwork()
        {
            //read networked hp and assign it to player data hp
            playerDataMono.HP = HP;

            //set position takes a bit time to complete (triggering by input is not enough),
            //therefore we need to handle it from fixedupdatenetwork
            if (pendingTeleportPosition.HasValue)
            {
                // Forcefully update KCC position BEFORE any movement/input logic
                KCC.SetPosition(pendingTeleportPosition.Value);
                pendingTeleportPosition = null;
            }

            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

        void CheckFallRespawn()
        {
            if (transform.position.y > -10) return;

            if (!Object.HasStateAuthority) return;

            StartCoroutine(RequestRespawn());
        }

        #region DEAD_AND_REVIVE_STATE_REMOTE
        public void OnStateChanged(NetworkBehaviourBuffer previous)
        {
            //get previous value and compare if player died or revived.
            var prevValue = GetPropertyReader<bool>(nameof(isDead)).Read(previous);

            //handle on death for the player.
            if (isDead)
                OnDeath();

            //check if the player was dead but is now alive in that case revive the player.
            else if (!isDead && prevValue)
                OnRevive();
        }
        private void OnDeath()
        {
            SetPlayerPropsToBeSync(false);

            //enable all slots to make drop action visible for remote players otherwise they can not call drop method
            //which is child of these slots
            for (int i = 0; i < weaponHolder.childCount; i++)
                weaponHolder.GetChild(i).gameObject.SetActive(true);

            //death prefab particle effect, it is destroyed after 2 seconds.
            Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
        }
        private void OnRevive()
        {
            SetPlayerPropsToBeSync(true);
        }
        #endregion

        #region TAKE_DAMAGE_SERVER
        //This function is called by other players in ShootManager script
        //Function only called on the server
        public void OnTakeDamage(NetworkId shooterNetworkId, byte damage)
        {
            //Only take damage while alive
            if (isDead) return;

            //get killer gameobject by killer network id
            GameObject shooterGameObject = Runner.FindObject(shooterNetworkId).gameObject;

            //null check
            if (shooterGameObject == null) return;

            //get killer player data component
            PlayerDataMono shooterPlayerData = shooterGameObject.GetComponent<PlayerDataMono>();

            //player data null check
            if (shooterPlayerData == null) return;

            //same team check, only enemy team player can give damage to us
            if (shooterPlayerData.playerData.team == playerDataMono.playerData.team) return;

            HP -= damage;//damage taken

            Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

            //player death check
            if (HP > 0 && HP <= 100) return;

            isDead = true;

            Debug.Log($"{Time.time} {transform.name} died");

            //leaderboard
            shooterPlayerData.AddKill();//increase killer's kill counter
            playerDataMono.AddDeath();//increase death player's death counter

            //killtable
            GameObject.FindAnyObjectByType<KillTableManager>()?.AddRawToKilltableRpc(
                shooterPlayerData.GetUsername(), playerDataMono.GetUsername());

            //team score for deathmatch
            GameObject.FindAnyObjectByType<Deathmatch>()?.UpdateTeamScoresRpc(shooterPlayerData.playerData.team);

            //respawn request
            StartCoroutine(RequestRespawn());
        }

        private void StartDiedState()
        {
            playerDataMono.playerState = PlayerState.Died;
            playerDataMono.playerStateStack.Add(playerDataMono.playerState);
        }
        private void ResetPlayerState()
        {
            playerDataMono.playerStateStack.Remove(PlayerState.Died);
            playerDataMono.playerStateStack.Remove(PlayerState.Reloading);
            playerDataMono.playerStateStack.Remove(PlayerState.Interacting);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
        }

        //respawn delay
        IEnumerator RequestRespawn()
        {

            StartDiedState();

            yield return new WaitForSeconds(2.0f);//time delay to respawn (2s)

            Respawn();
        }

        private void Respawn()
        {
            SpawnHandler _spawnHandler = FindObjectOfType<SpawnHandler>();

            if (_spawnHandler == null) return;

            Team team = playerDataMono.playerData.team;

            MovePlayerToSpawnPoint(_spawnHandler.GetSpawnPoint(team));

            OnRespawned();
        }

        private void MovePlayerToSpawnPoint(Vector3 spawnPoint)
        {
            pendingTeleportPosition = spawnPoint;
        }

        private void SetPlayerPropsToBeSync(bool state)
        {
            playerModel.gameObject.SetActive(state);
            hitboxRoot.HitboxRootActive = state;
            characterMovementHandler.SetCharacterControllerEnabled(state);
        }

        //Reset variables
        public void OnRespawned()
        {
            ResetPlayerState();

            HP = playerDataMono.startingHP;

            isDead = false;

            SetPlayerPropsToBeSync(true);

            //we enable all the slots to be able to drop all items when we die.
            //here, we disable other slots by calling switch slot method.
            itemSwitch.SwitchSlot(itemSwitch.currentSlot);
        }
        #endregion
    }
}

