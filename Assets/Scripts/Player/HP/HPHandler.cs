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

        public override void Spawned()
        {
            //set starting hp
            HP = playerDataMono.startingHP;
        }

        public override void FixedUpdateNetwork()
        {
            //read networked hp and assign it to player data hp
            playerDataMono.HP = HP;

            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

        void CheckFallRespawn()
        {
            if (transform.position.y > -10) return;

            if (!Object.HasStateAuthority) return;

            Respawn();
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
        //This function is called by other players and the changes in the hp value is handled above
        //Function only called on the server
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void OnTakeDamageRpc(NetworkId shooterNetworkId, byte damage)
        {
            //Only take damage while alive
            if (isDead) return;

            HP -= damage;//damage taken

            Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

            //Player not died
            if (HP > 0 && HP <= 100) return;

            //player died
            isDead = true;

            Debug.Log($"{Time.time} {transform.name} died");

            //get killer gameobject by killer network id
            GameObject killerGameObject = Runner.FindObject(shooterNetworkId).gameObject;

            //null check
            if (killerGameObject == null) return;

            //get killer player data component
            PlayerDataMono killerPlayerData = killerGameObject.GetComponent<PlayerDataMono>();

            //player data null check
            if (killerPlayerData == null) return;

            //leaderboard
            killerPlayerData.AddKill();//increase killer's kill counter
            playerDataMono.AddDeath();//increase death player's death counter

            //killtable
            GameObject.FindAnyObjectByType<KillTableManager>()?.AddRawToKilltableRpc(
                killerPlayerData.GetUsername(), playerDataMono.GetUsername());

            //team score for deathmatch
            GameObject.FindAnyObjectByType<Deathmatch>()?.UpdateTeamScoresRpc(killerPlayerData.playerData.team);

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

            yield return new WaitForSeconds(2.0f);

            Respawn();
        }

        private void Respawn()
        {
            SpawnHandler _spawnHandler = FindObjectOfType<SpawnHandler>();

            if (_spawnHandler == null) return;

            Team team = playerDataMono.team;

            if (team == Team.Soldier)
                MovePlayerToSpawnPoint(_spawnHandler.soldierSpawnPointContainer);

            else
                MovePlayerToSpawnPoint(_spawnHandler.alienSpawnPointContainer);

            OnRespawned();
        }

        private void MovePlayerToSpawnPoint(Transform spawnPointContainer)
        {
            int pointAmount = spawnPointContainer.childCount;

            int randomPointIndex = Random.Range(0, pointAmount);

            Vector3 spawnPoint = spawnPointContainer.GetChild(randomPointIndex).position;

            KCC.SetPosition(spawnPoint);
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

