using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
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
        [Networked]
        public byte HP { get; set; }//runtime hp value

        [Header("Player Datas")]
        [SerializeField] PlayerDataMono playerDataMono;
        [SerializeField] ItemSwitch itemSwitch;
        byte startingHP;//it is used when player respawned
        bool isInitialized = false;

        [Header("Killer Datas")]
        PlayerRef killerPlayer;
        GameObject killerGameObject;

        [Header("DeathUI")]
        public Color uiOnHitColor;
        public Image uiOnHitImage;

        [Header("Effects")]
        public GameObject playerModel;//our model will be invisivle when died and will be visible when revieved
        public GameObject deathGameObjectPrefab;//particle effect to play when died
        

        [Header("Other Components")]
        HitboxRoot hitboxRoot;
        CharacterMovementHandler characterMovementHandler;
        [SerializeField] InventoryManager inventoryManager;



        private void Awake()
        {
            characterMovementHandler = GetComponent<CharacterMovementHandler>();
            hitboxRoot = GetComponentInChildren<HitboxRoot>();
        }

        // Start is called before the first frame update
        void Start()
        {
            isDead = false;

            isInitialized = true;
        }
        public override void Spawned()
        {
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            startingHP = playerDataMono.HP;
            HP = playerDataMono.HP;
            Debug.Log(HP + " " + playerDataMono.HP);
        }

        public override void Render()
        {
            foreach (var change in changeDetector.DetectChanges(this, out var prev, out var current))
            {
                switch (change)
                {
                    case nameof(HP):
                        var hpReader = GetPropertyReader<byte>(nameof(HP));
                        var (oldHP, newHP) = hpReader.Read(prev, current);
                        OnHPChanged(oldHP, newHP);
                        break;

                    case nameof(isDead):
                        var stateReader = GetPropertyReader<bool>(nameof(isDead));
                        var (isDeadOld, isDeadCurrent) = stateReader.Read(prev, current);
                        OnStateChanged(isDeadOld, isDeadCurrent);
                        break;
                }
            }
        }

        #region HP_REDUCED_REMOTE
        public void OnHPChanged(byte oldHP, byte newHP)
        {
            Debug.Log($"{Time.time} OnHPChanged value {newHP} : " + HP);
            HP = newHP;
            //Check if the HP has been decreased
            if (newHP < oldHP)
                OnHPReduced();
        }
        private void OnHPReduced()
        {
            if (!isInitialized)
                return;
        }

        #endregion

        #region DEAD_STATE_REMOTE
        public void OnStateChanged(bool isDeadOld, bool isDeadCurrent)
        {
            Debug.Log($"{Time.time} OnStateChanged isDead {isDeadCurrent}");

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
            Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);

            //Öldü bilgisi ile inventory managerda metod çağıracak.
            inventoryManager.PlayerDied();
        }
        private void OnRevive()
        {
            Debug.Log($"{Time.time} OnRevive");

            if (Object.HasInputAuthority)
            {
                uiOnHitImage.color = new Color(0, 0, 0, 0);
            }
                

            playerModel.gameObject.SetActive(true);
            hitboxRoot.HitboxRootActive = true;
            characterMovementHandler.SetCharacterControllerEnabled(true);

            inventoryManager.PlayerRevived();
        }
        #endregion

        #region TAKE_DAMAGE_SERVER
        //This function is called by other players and the changes in the hp value is handled above
        //Function only called on the server
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void OnTakeDamageRpc(PlayerRef shooter, NetworkId shooterNetworkId, byte damage)
        {
            //Only take damage while alive
            if (isDead)
                return;

            HP -= damage;
            playerDataMono.HP-=damage;
            Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

            //Player died
            if (HP <= 0 || HP >100)
            {
                Debug.Log($"{Time.time} {transform.name} died");

                killerPlayer = shooter;
                //leaderboard
                killerGameObject = Runner.FindObject(shooterNetworkId).gameObject;
                killerGameObject.GetComponent<PlayerDataMono>()?.AddKill();
                playerDataMono.AddDeath();
                //killtable
                GameObject.FindAnyObjectByType<KillTableManager>()?.AddRawToKilltableRpc(
                killerGameObject.GetComponent<PlayerDataMono>()?.GetUsername(),
                playerDataMono.GetUsername());
                //team score for deathmatch
                GameObject.FindAnyObjectByType<Deathmatch>()?.UpdateTeamScoresRpc(playerDataMono.playerData.team);

                StartCoroutine(ServerReviveCO());
                
                isDead = true;
            }
        }

        IEnumerator ServerReviveCO()
        {
            playerDataMono.playerState = PlayerState.Died;
            playerDataMono.playerStateStack.Add(playerDataMono.playerState);
            yield return new WaitForSeconds(2.0f);

            characterMovementHandler.RequestRespawn();
        }

        public void OnRespawned()
        {
            //Reset variables
            playerDataMono.playerStateStack.Remove(PlayerState.Died);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
            HP = startingHP;
            isDead = false;
            itemSwitch.SwitchSlot(itemSwitch.currentSlot);
        }
        #endregion
    }
}

