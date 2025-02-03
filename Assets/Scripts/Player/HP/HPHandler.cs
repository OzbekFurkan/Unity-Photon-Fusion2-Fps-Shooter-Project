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
        [Header("Network Related Variables")]
        ChangeDetector changeDetector;
        [Networked]
        public bool isDead { get; set; }
        [Networked, OnChangedRender(nameof(OnHPChanged))]
        public byte HP { get; set; }//runtime hp value

        [Header("Player References")]
        [SerializeField] PlayerDataMono playerDataMono;
        [SerializeField] ItemSwitch itemSwitch;
        [SerializeField] SimpleKCC KCC;
        [SerializeField] InventoryManager inventoryManager;
        [SerializeField] Transform weaponHolder;
        HitboxRoot hitboxRoot;
        CharacterMovementHandler characterMovementHandler;
        

        [Header("Effects")]
        public GameObject playerModel;//our model will be invisible when died and will be visible when revieved
        public GameObject deathGameObjectPrefab;//particle effect to play when died

        private void Awake()
        {
            characterMovementHandler = GetComponent<CharacterMovementHandler>();
            hitboxRoot = GetComponentInChildren<HitboxRoot>();
        }

        public override void Spawned()
        {
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            isDead = false;
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
            if (transform.position.y < -10)
            {
                if (Object.HasStateAuthority)
                {
                    Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                    Respawn();
                }

            }
        }

        public override void Render()
        {
            foreach (var change in changeDetector.DetectChanges(this, out var prev, out var current))
            {
                switch (change)
                {
                    case nameof(isDead):
                        var stateReader = GetPropertyReader<bool>(nameof(isDead));
                        var (isDeadOld, isDeadCurrent) = stateReader.Read(prev, current);
                        OnStateChanged(isDeadOld, isDeadCurrent);
                        break;
                }
            }
        }

        #region HP_REDUCED_REMOTE
        public void OnHPChanged()
        {
            Debug.Log($"{Time.time} OnHPChanged value :" + HP);
            playerDataMono.HP = HP;
        }
        #endregion

        #region DEAD_STATE_REMOTE
        public void OnStateChanged(bool isDeadOld, bool isDeadCurrent)
        {
            //Handle on death for the player. Also check if the player was dead but is now alive in that case revive the player.
            if (isDeadCurrent)
                OnDeath();

            else if (!isDeadCurrent && isDeadOld)
                OnRevive();
        }
        private void OnDeath()
        {
            Debug.Log($"{Time.time} OnDeath");

            playerModel.gameObject.SetActive(false);
            hitboxRoot.HitboxRootActive = false;
            characterMovementHandler.SetCharacterControllerEnabled(false);

            for (int i = 0; i < weaponHolder.childCount; i++)
                weaponHolder.GetChild(i).gameObject.SetActive(true);

            Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
        }
        private void OnRevive()
        {
            Debug.Log($"{Time.time} OnRevive");

            playerModel.gameObject.SetActive(true);
            hitboxRoot.HitboxRootActive = true;
            characterMovementHandler.SetCharacterControllerEnabled(true);
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

        public void OnRespawned()
        {
            //Reset variables
            ResetPlayerState();

            HP = playerDataMono.startingHP;

            isDead = false;

            playerModel.gameObject.SetActive(true);
            hitboxRoot.HitboxRootActive = true;
            characterMovementHandler.SetCharacterControllerEnabled(true);

            itemSwitch.SwitchSlot(itemSwitch.currentSlot);
        }
        #endregion
    }
}

