using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.SimpleKCC;

namespace Player.Utils
{
    public class PlayerReferenceGetter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerDataMono playerDataMono;
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private HPHandler hpHandler;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private CharacterInputHandler characterInputHandler;
        [SerializeField] private Transform playerCameraHandle;
        [SerializeField] private Transform playerCameraPivot;
        [SerializeField] private SimpleKCC playerKCC;

        public PlayerDataMono GetPlayerDataMono()
        {
            return playerDataMono;
        }
        public ItemSwitch GetItemSwitch()
        {
            return itemSwitch;
        }
        public HPHandler GetHPHandler()
        {
            return hpHandler;
        }
        public Transform GetWeaponHolder()
        {
            return weaponHolder;
        }
        public CharacterInputHandler GetCharacterInputHandler()
        {
            return characterInputHandler;
        }
        public Transform GetPlayerCameraHandle()
        {
            return playerCameraHandle;
        }
        public Transform GetPlayerCameraPivot()
        {
            return playerCameraPivot;
        }
        public SimpleKCC GetPlayerKCC()
        {
            return playerKCC;
        }
    }
}

