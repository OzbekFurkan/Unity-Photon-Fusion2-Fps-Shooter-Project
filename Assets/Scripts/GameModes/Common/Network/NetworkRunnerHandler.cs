using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;
using Player;
using Menu;
using GameModes.Modes;

namespace GameModes.Common
{
    public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
    {

        NetworkRunner networkRunner;

        JoinRoomHandler joinRoomHandler;

        MenuLifeCycleHandler menuLifeCycleHandler;

        #region USED_NETWORK_RUNNER_CALLBACKS
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (joinRoomHandler == null)
                return;

            if(sessionList.Count>0)
            {
                joinRoomHandler.ClearRoomList();
                foreach(SessionInfo sessionInfo in sessionList)
                {
                    joinRoomHandler.AddRoomToRoomList(sessionInfo);
                }
            }

        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("OnShutdown");
            SceneManager.LoadScene("MainMenu");
        }
        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("OnDisconnectedFromServer");
            // Shuts down the local NetworkRunner when the client is disconnected from the server.
            GetComponent<NetworkRunner>().Shutdown();
        }
        #endregion

        #region COMPONENT_ASSIGNMENTS
        void Awake()
        {
            joinRoomHandler = FindObjectOfType<JoinRoomHandler>();
            menuLifeCycleHandler = FindObjectOfType<MenuLifeCycleHandler>();
        }

        // Start is called before the first frame update
        void Start()
        {
            networkRunner = GetComponent<NetworkRunner>();
            networkRunner.name = "Network runner";

            //We will start session only in the menu scene.
            //networkrunner prefab will survive on scene changes because it is 'dont destroy on load'
            
        }
        #endregion

        #region START_SESSION
        INetworkSceneManager GetSceneManager(NetworkRunner runner)
        {
            var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
            if(sceneManager == null)
            {
                sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            }
            return sceneManager;
        }

        async void StartGame(NetworkRunner runner, GameMode gameMode, string roomName, string sceneName, int maxPlayers)
        {
            
            ToggleStatusPanel();//loading on

            var sceneManager = GetSceneManager(runner);
            runner.ProvideInput = true;

            Dictionary<string, SessionProperty> sessionProps= new Dictionary<string, SessionProperty>();
            sessionProps.Add("sceneName", sceneName);

            await runner.StartGame(new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = roomName,
                SessionProperties = sessionProps,
                PlayerCount = maxPlayers
            });
            
            ToggleStatusPanel();//loading off
            
            if (runner.IsServer)
            {
                await runner.LoadScene(sceneName);
                
                NetworkObject[] _no = FindObjectsOfType<NetworkObject>();
                runner.RegisterSceneObjects(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), _no);
            }
            
        }
        #endregion

        #region MENU_ACTIONS
        //toggles loading animation screen
        private void ToggleStatusPanel()
        {
            if (menuLifeCycleHandler == null)
                return;
            
            menuLifeCycleHandler.statusPanel.SetActive(!menuLifeCycleHandler.statusPanel.activeSelf);

        }
        public void OnJoinLobby()
        {
            var clientTask = JoinLobby();
        }
        private async Task JoinLobby()
        {
            Debug.Log("Joining Lobby...");

            ToggleStatusPanel();//loading on

            string lobbyName = "GameLobby";
            var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
            if(!result.Ok)
            {
                Debug.Log("Failed while joining lobby");
                //failed to connect panel needed here!
            }
            else
            {
                Debug.Log("joined lobby");
                ToggleStatusPanel();//loading off
            }
        }


        public void CreateRoom(string roomName, string sceneName, int maxPlayers)
        {
            StartGame(networkRunner, GameMode.Host, roomName, sceneName, maxPlayers);
        }
        public void JoinRoom(SessionInfo sessionInfo)
        {
            sessionInfo.Properties.TryGetValue("sceneName", out SessionProperty sceneName);
            if(sceneName!=null)
                StartGame(networkRunner, GameMode.Client, sessionInfo.Name, sceneName, sessionInfo.MaxPlayers);
        }
        #endregion



        #region UNUSED_RUNNER_CALLBACKS
        public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("OnConnectedToServer"); }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectRequest"); }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        #endregion

    }
}

