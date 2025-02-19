using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Player;
using Utilitiy;

namespace GameModes.Common
{
    public class SpawnHandler : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Header("Player Prefabs For Each Teams")]
        public Player.NetworkPlayer SoldierPlayerPrefab;
        public Player.NetworkPlayer AlienPlayerPrefab;

        [Header("Spawn Points")]
        public Transform soldierSpawnPointContainer;
        public Transform alienSpawnPointContainer;

        [HideInInspector] public Player.NetworkPlayer LocalPlayer { get; private set; }
        [Networked] private int selectedTeam { get; set; }

        public override void Spawned()
        {
            selectedTeam = 1;
        }

        public override void Render()
        {
            // Prepare LocalPlayer property that can be accessed from UI
            if (LocalPlayer == null || LocalPlayer.Object == null || LocalPlayer.Object.IsValid == false)
            {
                var playerObject = Runner.GetPlayerObject(Runner.LocalPlayer);
                LocalPlayer = playerObject != null ? playerObject.GetComponent<Player.NetworkPlayer>() : null;
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            NetworkRunner runner = Runner;
            if (runner.IsServer)
            {
                Debug.Log("OnPlayerJoined we are server. Spawning player");
                Player.NetworkPlayer spawnedPlayer;
                if (selectedTeam == (int)Team.Soldier)
                {
                    spawnedPlayer = runner.Spawn(SoldierPlayerPrefab, GetSpawnPoint(), Quaternion.identity, player);
                    selectedTeam = (int)Team.Alien;
                    runner.SetPlayerObject(player, spawnedPlayer.Object);
                }
                else
                {
                    spawnedPlayer = runner.Spawn(AlienPlayerPrefab, GetSpawnPoint(), Quaternion.identity, player);
                    selectedTeam = (int)Team.Soldier;
                    runner.SetPlayerObject(player, spawnedPlayer.Object);
                } 

            }
            else Debug.Log("OnPlayerJoined");
        }

        /// <summary>Returns a random spawn point. Team sensitive.</summary>
        public Vector3 GetSpawnPoint()
        {
            if(selectedTeam == (int)Team.Soldier)
            {
                int i = Random.Range(0, soldierSpawnPointContainer.childCount);
                return soldierSpawnPointContainer.GetChild(i).position;
            }
            else
            {
                int i = Random.Range(0, alienSpawnPointContainer.childCount);
                return soldierSpawnPointContainer.GetChild(i).position;
            }
        }

        public void PlayerLeft(PlayerRef player)
        {
            if(Runner.IsServer)
            {
                NetworkObject NO = Runner.GetPlayerObject(player);
                Runner.Despawn(NO);
            }
        }
    }
}