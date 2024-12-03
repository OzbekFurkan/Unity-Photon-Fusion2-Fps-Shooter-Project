using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using GameModes.Common;

namespace Menu
{
    public class JoinRoomHandler : MonoBehaviour
    {
        [Header("RoomList Elements")]
        public Transform roomListVerticalLayout;
        public GameObject roomButtonPrefab;

        ///<summary>Destroys all the join room buttons on the room list</summary>
        public void ClearRoomList()
        {
            foreach (Transform roomButton in roomListVerticalLayout.transform)
            {
                if (roomButton == roomListVerticalLayout.transform.GetChild(0))
                    continue;

                Destroy(roomButton);
            }
        }
        ///<summary>Spawns room button and sets room data values into spawned room button</summary>
        public void AddRoomToRoomList(SessionInfo sessionInfo)
        {
            SessionDataMono sessionDataMono = Instantiate(roomButtonPrefab, roomListVerticalLayout).GetComponent<SessionDataMono>();
            sessionDataMono.SetSessionData(sessionInfo);
            sessionDataMono.OnJoinSession += OnJoinButtonPressed;
        }

        //This method will get called when we pressed a room button and it will send us its session info via OnJoinSession event
        public void OnJoinButtonPressed(SessionInfo sessionInfo)
        {
            NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
            networkRunnerHandler.JoinRoom(sessionInfo);
        }


    }
}