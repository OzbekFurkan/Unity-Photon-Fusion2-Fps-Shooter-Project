using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using Player;
using Item;
using UnityEngine.UI;
using Network;

namespace ProjectUI
{
    public class PlayerUIManager : NetworkBehaviour
    {
        [Header("PlayerData")]
        [SerializeField] PlayerDataMono playerDataMono;
        [SerializeField] GameObject playerUi;
        [SerializeField] ItemSwitch itemSwitch;
        [SerializeField] HPHandler hPHandler;
        [SerializeField] Transform weaponHolder;

        [Header("Weapon Data UI")]
        [SerializeField] TextMeshProUGUI ammoText; 

        [Header("Hp Data UI")]
        [SerializeField] Slider hpBarSlider;

        [Header("SlotUI")]
        [SerializeField] GameObject slotUI;
        [SerializeField] Sprite blankSlotIcon;

        [Header("Leaderboard UI")]
        public Transform leaderboard;
        public GameObject leaderboardRaw;

        public override void Spawned()
        {
            if (!Object.HasInputAuthority)
                playerUi.SetActive(false);
        }

        // Update is called once per frame
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
                return;

            //playerUI
            SetAmmoText();
            SetHpBarSlider();
            SetSlotUI();

            //InputGameUI
            HandleInput();
               
        }
        #region AMMO
        private void SetAmmoText()
        {
            if (weaponHolder.GetChild(itemSwitch.currentSlot).childCount > 0)
            {
                weaponHolder.GetChild(itemSwitch.currentSlot).GetChild(0).gameObject.TryGetComponent<WeaponDataMono>(out var weaponData);

                if (weaponData != null && weaponData.isActiveAndEnabled && weaponData.Object != null)
                {
                    int ammoData = weaponData.ammo;
                    ammoText.text = ammoData + "";
                }
                else
                {
                    ammoText.text = "0";
                }
            }
            else
            {
                ammoText.text = "0";
            }
        }
        #endregion

        #region HP
        private void SetHpBarSlider()
        {
            hpBarSlider.value = hPHandler.HP;
        }
        #endregion

        #region SLOT
        private void SetSlotUI()
        {
            //SetAllIcons
            for (int i = 0; i < weaponHolder.childCount - 1/*slot amount, 'Other' excluded by '-1'*/; i++)
            {
                //empty slots setted as blank icon
                if (weaponHolder.GetChild(i).childCount <= 0)
                {
                    slotUI.transform.GetChild(i).gameObject.TryGetComponent<Image>(out Image image);
                    if (image != null)
                    {
                        image.sprite = blankSlotIcon;
                        //dark-gray overlay by default
                        image.color = SetOverlayOnSlotIcons("#858585");
                    }
                }
                //non-empty slots setted as their own icons
                else
                {
                    GameObject item = weaponHolder.GetChild(i).GetChild(0).gameObject;
                    item.TryGetComponent<ItemDataMono>(out ItemDataMono itemData);
                    if(itemData != null)
                    {
                        slotUI.transform.GetChild(i).gameObject.TryGetComponent<Image>(out Image image);
                        if(image != null)
                        {
                            image.sprite = itemData.itemIcon;
                            //dark-gray overlay by default
                            image.color = SetOverlayOnSlotIcons("#858585");
                        }
                    }
                }

            }

            //Set Current Slot Overlay White
            slotUI.transform.GetChild(itemSwitch.currentSlot).gameObject.TryGetComponent<Image>(out Image currentSlotImage);
            if(currentSlotImage != null)
            {
                currentSlotImage.color = SetOverlayOnSlotIcons("#FFFFF");
            }

        }
        private Color SetOverlayOnSlotIcons(string htmlValue)
        {
            Color convertedColor = Color.white;
            ColorUtility.TryParseHtmlString(htmlValue, out convertedColor);
            return convertedColor;
        }
        #endregion

        #region LEADERBOARD
        private void HandleInput()
        {
            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isLeaderboardButtonPressed)
                {
                    leaderboard.gameObject.SetActive(true);
                    InstantiateAllLeaderboardRaws();
                }
                else if (!networkInputData.isLeaderboardButtonPressed)
                {
                    leaderboard.gameObject.SetActive(false);
                }

            }
        }    

        public void InstantiateAllLeaderboardRaws()
        {

            DestroyLeaderboardItems();
            foreach (PlayerDataStruct playerData in GetAllPlayersDatas())
            {
                GameObject newRaw = Instantiate(leaderboardRaw, Vector3.zero, Quaternion.identity);
                newRaw.transform.SetParent(leaderboard);
                newRaw.transform.localScale = Vector3.one;
                newRaw.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerData.username.ToString();
                newRaw.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = playerData.kill + "";
                newRaw.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = playerData.death + "";
            }

        }
        public void DestroyLeaderboardItems()
        {
            for (int i = 0; i < leaderboard.childCount; i++)
            {
                if (i == 0 || i == 1)
                    continue;

                Destroy(leaderboard.GetChild(i).gameObject);
            }
        }
        public List<PlayerDataStruct> GetAllPlayersDatas()
        {
            List<PlayerDataStruct> playerDatas = new List<PlayerDataStruct>();

            PlayerDataMono[] playerDataMonos = GameObject.FindObjectsByType<PlayerDataMono>(FindObjectsSortMode.None);
            foreach (PlayerDataMono playerDataMono in playerDataMonos)
            {
                playerDatas.Add(playerDataMono.playerData);
            }

            return playerDatas;
        }
        #endregion

    }
}

