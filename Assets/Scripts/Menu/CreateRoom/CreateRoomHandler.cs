using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using GameModes.Common;

namespace Menu
{
    public class CreateRoomHandler : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image mapIcon;
        public TMP_Dropdown mapDropdown;
        public TMP_InputField roomNameInput;
        public TMP_Dropdown maxPlayerDropdown;

        [Header("Room Data")]
        private string roomName;
        private int maxPlayer;
        private int selectedMapId;

        [Header("Maps")]
        public MapSettings[] maps;


        // Start is called before the first frame update
        void Start()
        {
            //map dropdown
            OrderMapsById();
            InitializeMapDropdownOptions();
            SetMapData();
            //max player
            SetMaxPlayerData();
        }

        #region MAP_DROPDOWN
        private void OrderMapsById()
        {
            //bubble sort ascending order
            for (int i = 0; i<maps.Length-1; i++)
            {
                for(int j = 0; i < maps.Length-i-1; i++)
                {
                    if(maps[j].mapId > maps[j+1].mapId)
                    {
                        MapSettings tempSmall = maps[j + 1];//store small one temporarily
                        maps[j+1] = maps[j];//assign big one to the small one
                        maps[j] = tempSmall;//assign small one to lower index
                    }
                }
            }
        }
        private void InitializeMapDropdownOptions()
        {
            List<string> mapOptions = new List<string>();
            foreach (MapSettings map in maps)
            {
                mapOptions.Add(map.mapName);
            }
            mapDropdown.AddOptions(mapOptions);
        }
        private void SetMapData()
        {
            selectedMapId = mapDropdown.value;
            MapSettings mapSettings = FindSelectedMapSettings(selectedMapId);
            if(mapSettings != null)
            {
                mapIcon.sprite = mapSettings.mapIcon;
            }
        }
        private MapSettings FindSelectedMapSettings(int mapId)
        {
            foreach(MapSettings map in maps)
            {
                if ((int)map.mapId == mapId)
                    return map;
            }
            return null;
        }
        public void OnMapDropdownChanged()
        {
            SetMapData();
        }
        #endregion

        #region MAX_PLAYER
        private void SetMaxPlayerData()
        {
            List<TMP_Dropdown.OptionData> options = maxPlayerDropdown.options;
            TMP_Dropdown.OptionData option = options[maxPlayerDropdown.value];
            maxPlayer = int.Parse(option.text);
        }
        public void OnMaxPlayerDropdownChanged()
        {
            SetMaxPlayerData();
        }
        #endregion

        public void CreateRoom()
        {
            NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
            networkRunnerHandler.CreateRoom(roomNameInput.text, FindSelectedMapSettings(selectedMapId).mapName, maxPlayer);
        }

    }
}