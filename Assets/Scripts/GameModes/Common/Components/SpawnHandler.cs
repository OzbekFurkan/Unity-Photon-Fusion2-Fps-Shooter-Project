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

        [Networked] private int selectedTeam { get; set; }

        public override void Spawned()
        {
            selectedTeam = 0;
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
                    spawnedPlayer = runner.Spawn(SoldierPlayerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
                    selectedTeam = (int)Team.Alien;
                    runner.SetPlayerObject(player, spawnedPlayer.Object);
                }
                else
                {
                    spawnedPlayer = runner.Spawn(AlienPlayerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
                    selectedTeam = (int)Team.Soldier;
                    runner.SetPlayerObject(player, spawnedPlayer.Object);
                } 
                if (spawnedPlayer.HasInputAuthority)
                {
                    Camera.main.gameObject.SetActive(false);
                }

            }
            else Debug.Log("OnPlayerJoined");
        }

        public void PlayerLeft(PlayerRef player)
        {
            
        }
    }
}