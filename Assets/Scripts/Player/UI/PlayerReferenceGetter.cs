using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}

