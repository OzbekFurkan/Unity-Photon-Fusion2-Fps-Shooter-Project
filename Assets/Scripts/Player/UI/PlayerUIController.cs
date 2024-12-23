using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameModes.Common;
using Fusion;
using UnityEngine.UI;
using Item;

namespace Player.UI
{
    public class PlayerUIController : MonoBehaviour
    {
        
        [Header("References")]
        [SerializeField] SpawnHandler spawnHandler;
        [SerializeField] CanvasGroup playerUI;
        PlayerDataMono playerDataMono;
        ItemSwitch itemSwitch;
        HPHandler hpHandler;
        Transform weaponHolder;
        CharacterInputHandler characterInputHandler;

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
        bool isLeaderBoardbuttonPressed = false;//To prevent calling leaderboard diplay method multiple times

        [Header("PauseMenu")]
        [SerializeField] private GameObject pauseMenuPanel;

        public void Update()
        {
            if (spawnHandler.LocalPlayer == null)
            {
                playerUI.alpha = 0;
                return;
            }
            playerUI.alpha = 1;
            InitializeReferences();

            SetAmmoText();
            SetHpBarSlider();
            SetSlotUI();

            HandleInput();

        }

        private void InitializeReferences()
        {
            spawnHandler.LocalPlayer.TryGetComponent<PlayerReferenceGetter>(out PlayerReferenceGetter playerReferenceGetter);
            if(playerReferenceGetter != null)
            {
                playerDataMono = playerReferenceGetter.GetPlayerDataMono();
                itemSwitch = playerReferenceGetter.GetItemSwitch();
                hpHandler = playerReferenceGetter.GetHPHandler();
                weaponHolder = playerReferenceGetter.GetWeaponHolder();
                characterInputHandler = playerReferenceGetter.GetCharacterInputHandler();
            }
        }

        #region AMMO
        private void SetAmmoText()
        {
            if (weaponHolder.GetChild(itemSwitch.currentSlot).childCount > 0)
            {
                weaponHolder.GetChild(itemSwitch.currentSlot).GetChild(0).gameObject.TryGetComponent<WeaponDataMono>(out var weaponData);

                if (weaponData != null && weaponData.isActiveAndEnabled && weaponData.Object != null)
                {
                    if (playerDataMono.playerState == PlayerState.Reloading)
                    {
                        ammoText.text = "Reloading...";
                        return;
                    }
                    int ammoData = weaponData.ammo;
                    int fullAmmoData = weaponData.fullAmmo;
                    ammoText.text = ammoData + "/" + fullAmmoData;
                }
                else
                {
                    ammoText.text = "";
                }
            }
            else
            {
                ammoText.text = "";
            }
        }
        #endregion

        #region HP
        private void SetHpBarSlider()
        {
            hpBarSlider.value = hpHandler.HP;
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
                    if (itemData != null)
                    {
                        slotUI.transform.GetChild(i).gameObject.TryGetComponent<Image>(out Image image);
                        if (image != null)
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
            if (currentSlotImage != null)
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

        #region PlayerInputUI
        private void HandleInput()
        {
            if (characterInputHandler.isLeaderboardButtonPressed && !isLeaderBoardbuttonPressed)
            {
                isLeaderBoardbuttonPressed = true;
                leaderboard.gameObject.SetActive(true);
                InstantiateAllLeaderboardRaws();
            }
            else if (!characterInputHandler.isLeaderboardButtonPressed)
            {
                leaderboard.gameObject.SetActive(false);
                isLeaderBoardbuttonPressed = false;
            }
            if (characterInputHandler.isPauseButtonPressed)
            {
                pauseMenuPanel.SetActive(true);
                playerDataMono.playerState = PlayerState.Paused;
                playerDataMono.playerStateStack.Add(playerDataMono.playerState);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                pauseMenuPanel.SetActive(false);
                playerDataMono.playerStateStack.Remove(PlayerState.Paused);
                playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

        }
        #endregion

        #region LEADERBOARD
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

        #region PAUSED_PANEL
        public void ResumeButtonClicked()
        {
            Debug.Log("resume clicked");
            if (characterInputHandler != null)
                characterInputHandler.isPauseButtonPressed = !characterInputHandler.isPauseButtonPressed;
        }
        public void SettingsButtonClicked()
        {

        }
        public void QuitButtonClicked()
        {
            FindObjectOfType<NetworkRunner>().Shutdown();
        }
        #endregion

    }
}