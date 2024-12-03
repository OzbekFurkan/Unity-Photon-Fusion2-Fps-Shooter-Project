using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameModes.Common;

namespace Menu
{
    public class MenuLifeCycleHandler : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject usernamePanel;
        public GameObject createRoomPanel;
        public GameObject joinRoomPanel;
        public GameObject settingsPanel;
        public GameObject statusPanel;

        [Header("UI Elements")]
        public TMP_InputField usernameInput;

        // Start is called before the first frame update
        void Start()
        {
            InitializePanels();
        }

        private void DisableAllPanels()
        {
            usernamePanel.SetActive(false);
            createRoomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        private void InitializePanels()
        {
            DisableAllPanels();
            //First panel (username) displayed
            usernamePanel.SetActive(true);
        }

        public void PlayButtonClickedInUsernamePanel()
        {
            PlayerPrefs.SetString("username", usernameInput.text);
            usernamePanel.SetActive(false);

            //Join the lobby when username panel skipped by pressing play button
            NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
            networkRunnerHandler.OnJoinLobby();
        }

        public void OpenCreateRoomPanel()
        {
            DisableAllPanels();
            createRoomPanel.SetActive(true);
        }
        public void OpenJoinRoomPanel()
        {
            DisableAllPanels();
            joinRoomPanel.SetActive(true);
        }
        public void OpenSettingsPanel()
        {
            DisableAllPanels();
            settingsPanel.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}