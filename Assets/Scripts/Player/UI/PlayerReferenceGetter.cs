using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.SimpleKCC;

namespace Player.UI
{
    public class PlayerReferenceGetter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerDataMono playerDataMono;
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private HPHandler hpHandler;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private CharacterInputHandler characterInputHandler;
        [SerializeField] private Transform playerCamera;
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
        public Transform GetPlayerCamera()
        {
            return playerCamera;
        }
        public SimpleKCC GetPlayerKCC()
        {
            return playerKCC;
        }
    }
}

