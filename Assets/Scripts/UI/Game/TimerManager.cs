using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

namespace ProjectUI
{
    public class TimerManager : NetworkBehaviour
    {
        public TextMeshProUGUI timerText; // Reference to the TextMeshProUGUI component
        [Tooltip("Starting Time In Seconds")]public float startTime = 60f; // Starting time in seconds

        [Networked] public float currentTime { get; set; }
        [Networked] public NetworkBool isTİmeUp { get; set; }

        public override void Spawned()
        {
            currentTime = startTime;
            isTİmeUp = false;
            StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            while (currentTime > 0)
            {
                // Update timer text
                UpdateTimerDisplay();

                // Wait for one second
                yield return new WaitForSeconds(1f);

                // Reduce the timer
                currentTime -= 1f;
            }

            // Final update to set the timer to 0 and handle what happens when time is up
            currentTime = 0;
            UpdateTimerDisplay();
            TimerEnded();
        }

        private void UpdateTimerDisplay()
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        private void TimerEnded()
        {
            Debug.Log("Time's up!");
            isTİmeUp = true;
            // Add logic for what happens when the timer ends
        }
    }
}

