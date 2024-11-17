using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ProjectUI;
using Player;
using TMPro;

namespace ProjectGameMode
{
    [RequireComponent(typeof(NetworkObject), typeof(KillTableManager), typeof(TimerManager))]
    public class Deathmatch : NetworkBehaviour
    {
        [Header("TeamScore")]
        [SerializeField] private TextMeshProUGUI teamScoreTextA;
        [SerializeField] private TextMeshProUGUI teamScoreTextB;
        [Networked, OnChangedRender(nameof(OnTeamScoreChanged))] public int teamScoreA { get; set; }
        [Networked, OnChangedRender(nameof(OnTeamScoreChanged))] public int teamScoreB { get; set; }

        [Header("TimerCheck")]
        [SerializeField] private TimerManager timerManager;

        [Header("GameOver")]
        [SerializeField] GameObject GameOverUI;

        public void OnTeamScoreChanged()
        {
            teamScoreTextA.text = teamScoreA+"";
            teamScoreTextB.text = teamScoreB+"";
        }

        public override void Spawned()
        {
            teamScoreA = 0;
            teamScoreB = 0;
        }
        public override void FixedUpdateNetwork()
        {
            if (timerManager.isTİmeUp)
                EndGame();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
        public void UpdateTeamScoresRpc(Team team)
        {
            if (team == Team.Blue)
                teamScoreA++;
            else if (team == Team.Red)
                teamScoreB++;
        }
        public void EndGame()
        {
            //gameover ui enable
            GameOverUI.SetActive(timerManager.isTİmeUp);
        }
    }
}

