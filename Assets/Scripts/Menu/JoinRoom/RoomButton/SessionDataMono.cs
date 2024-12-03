using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Fusion;
using TMPro;

namespace Menu
{
    public class SessionDataMono : MonoBehaviour
    {
        [Header("Room Button UI Elements")]
        public TextMeshProUGUI roomName;
        public TextMeshProUGUI numberOfPlayer;
        public TextMeshProUGUI mapName;

        [Header("Room Data")]
        [HideInInspector] public SessionInfo sessionInfo;

        //Event
        public event Action<SessionInfo> OnJoinSession;

        ///<summary>Sets room data values into UI elements of room button</summary>
        public void SetSessionData(SessionInfo sessionInfo)
        {
            this.sessionInfo = sessionInfo;

            sessionInfo.Properties.TryGetValue("sceneName", out SessionProperty sceneName);

            roomName.text = sessionInfo.Name;
            numberOfPlayer.text = sessionInfo.PlayerCount + " / " + sessionInfo.MaxPlayers;
            mapName.text = sceneName;

            if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
                GetComponent<Button>().interactable = false;


        }

        public void OnRoomButtonClick()
        {
            OnJoinSession?.Invoke(sessionInfo);
        }

    }
}