using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using GameModes.Common;
using Player;
using TMPro;
using UnityEngine.UI;

namespace GameModes.Modes
{
    
    public sealed class Deathmatch : BaseGameMode
    {
        [Header("TeamScore")]
        [SerializeField] private TextMeshProUGUI teamScoreTextA;
        [SerializeField] private TextMeshProUGUI teamScoreTextB;
        [Networked, OnChangedRender(nameof(OnTeamScoreChanged))] public int teamScoreSoldier { get; set; }
        [Networked, OnChangedRender(nameof(OnTeamScoreChanged))] public int teamScoreAlien { get; set; }

        [Header("Timer")]
        [Tooltip("Starting Time In Seconds")] public float startTime = 60f; // Starting time in seconds
        [Networked, HideInInspector] public TickTimer countdownTimer { get; set; }
        public TextMeshProUGUI timerText; // Reference to the timer text component

        [Header("Gameover")]
        [SerializeField] GameObject GameOverUI;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private Image winnerTeamIcon;
        [SerializeField] private Sprite soldierTeamIcon;
        [SerializeField] private Sprite alienTeamIcon;
        [SerializeField] private Sprite drawIcon;
        [SerializeField] TextMeshProUGUI countdownForMenuText; // Reference to the timer for menu text component

        #region TEAM_SCORE
        public void OnTeamScoreChanged()
        {
            teamScoreTextA.text = teamScoreSoldier+"";
            teamScoreTextB.text = teamScoreAlien+"";
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
        public void UpdateTeamScoresRpc(Team team)
        {
            if (team == Team.Soldier)
                teamScoreSoldier++;
            else if (team == Team.Alien)
                teamScoreAlien++;
        }
        #endregion

        public override void Spawned()
        {
            //set initial team scores
            teamScoreSoldier = 0;
            teamScoreAlien = 0;

            // Set is Simulated so that FixedUpdateNetwork runs on every client instead of just the Host
            Runner.SetIsSimulated(Object, true);

            // --- This section is for all networked information that has to be initialized by the HOST
            if (!Object.HasStateAuthority) return;
            //set game state
            gameState = GameState.Playing;
            //start timer
            StartCountDown(startTime);
        }
        #region TIMER
        public override void FixedUpdateNetwork()
        {
            switch (gameState)
            {
                case GameState.Playing:
                    UpdatePlayingTimerText((float)countdownTimer.RemainingTime(Runner));
                    // Ends the game if the game session length has been exceeded
                    if (countdownTimer.ExpiredOrNotRunning(Runner))
                    {
                        TimerEnded();
                    }
                    break;
                case GameState.Ended:
                    UpdateEndingTimerText((float)countdownTimer.RemainingTime(Runner));
                    break;
            }
        }

        private void StartCountDown(float startTime)
        {
            countdownTimer = TickTimer.CreateFromSeconds(Runner, startTime);
        }
        private void UpdatePlayingTimerText(float currentTime)
        {
            UpdateTimerDisplay(currentTime);
        }

        private void UpdateEndingTimerText(float currentTime)
        {
            countdownForMenuText.text = (int)currentTime + "";

            // --- Host
            // Shutdowns the current game session.
            // The disconnection behaviour is found in the NetworkRunnerHandler.cs script
            if (countdownTimer.ExpiredOrNotRunning(Runner) == false) return;

            Runner.Shutdown();

        }

        private void UpdateTimerDisplay(float currentTime)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        private void TimerEnded()
        {
            Debug.Log("Time's up!");
            gameState = GameState.Ended;
            //enable gameover ui
            GameOverUI.SetActive(true);
            //decide winner
            DecideWinner();
            //Set countdownTimerForMenu
            StartCountDown(10);
        }

        public void DecideWinner()
        {
            //decide winner
            if(teamScoreSoldier > teamScoreAlien)
            {
                //soldier win
                winnerText.text = "Soldier Won";
                winnerTeamIcon.sprite = soldierTeamIcon;
            }
            else if(teamScoreAlien > teamScoreSoldier)
            {
                //alien win
                winnerText.text = "Alien Won";
                winnerTeamIcon.sprite = alienTeamIcon;
            }
            else
            {
                //draw
                winnerText.text = "Draw";
                winnerTeamIcon.sprite = drawIcon;
            }
        }
        #endregion
    }
}

