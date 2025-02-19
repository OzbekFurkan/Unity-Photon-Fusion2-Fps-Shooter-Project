using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameModes.Common;
using Fusion;
using UnityEngine.UI;
using Player.Utils;
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

        [Header("OnHitUI")]
        private byte prevHP=100;
        public Color uiOnHitColor;
        public Image uiOnHitImage;

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

            //null check of references
            if (playerDataMono == null || itemSwitch == null || hpHandler == null ||
                weaponHolder == null || weaponHolder == null) return;

            SetAmmoText();
            SetHpBarSlider();
            //ShowHitEffectOnHit();
            SetSlotUI();

            HandleInput();

        }

        private void InitializeReferences()
        {
            spawnHandler.LocalPlayer.TryGetComponent<PlayerReferenceGetter>(out PlayerReferenceGetter playerReferenceGetter);

            if (playerReferenceGetter == null) return;

            //set references
            playerDataMono = playerReferenceGetter.GetPlayerDataMono();
            itemSwitch = playerReferenceGetter.GetItemSwitch();
            hpHandler = playerReferenceGetter.GetHPHandler();
            weaponHolder = playerReferenceGetter.GetWeaponHolder();
            characterInputHandler = playerReferenceGetter.GetCharacterInputHandler();
        }

        #region AMMO
        private void SetAmmoText()
        {
            Transform _currentSlot = weaponHolder.GetChild(itemSwitch.currentSlot);

            //make it empty if there is no item in the current slot
            if(_currentSlot.childCount <= 0)
            {
                ammoText.text = "";
                return;
            }

            _currentSlot.GetChild(0).gameObject.TryGetComponent<WeaponDataMono>(out var weaponData);

            //weapon data null checks, make it empty if it is
            if(weaponData == null || !weaponData.isActiveAndEnabled || weaponData.Object == null)
            {
                ammoText.text = "";
                return;
            }

            //write reloading while reloding and return (this reloading thing is optional)
            if (playerDataMono.playerState == PlayerState.Reloading)
            {
                ammoText.text = "Reloading...";
                return;
            }

            //set ammo values
            int ammoData = weaponData.ammo;
            int fullAmmoData = weaponData.fullAmmo;
            ammoText.text = ammoData + "/" + fullAmmoData;


        }
        #endregion

        #region HP
        private void SetHpBarSlider()
        {
            hpBarSlider.value = hpHandler.HP;
        }
        #endregion

        #region ON_HIT
        private void ShowHitEffectOnHit()
        {
            if(hpHandler.HP < 100 && hpHandler.HP < prevHP)
            {
                uiOnHitImage.color = uiOnHitColor;
                prevHP = hpHandler.HP;
            }
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
                    SetSlotIcon(i);

                }
                //non-empty slots setted as their own icons
                else
                {
                    GameObject item = weaponHolder.GetChild(i).GetChild(0).gameObject;
                    item.TryGetComponent<ItemDataMono>(out ItemDataMono itemData);

                    if (itemData == null) return;

                    SetSlotIcon(i, itemData.itemDataSettings.itemIcon);
                }

            }

            //Set Current Slot Overlay As White
            slotUI.transform.GetChild(itemSwitch.currentSlot).gameObject.TryGetComponent<Image>(out Image currentSlotImage);

            if (currentSlotImage == null) return;

            currentSlotImage.color = SetOverlayOnSlotIcons("#FFFFF");
        }
        private void SetSlotIcon(int slot, Sprite itemIcon = null)
        {
            if (itemIcon == null)
                itemIcon = blankSlotIcon;

            slotUI.transform.GetChild(slot).gameObject.TryGetComponent<Image>(out Image image);

            if (image == null) return;

            image.sprite = itemIcon;
            //dark-gray overlay by default
            image.color = SetOverlayOnSlotIcons("#858585");

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
                StartPausedState();
                SetCursorState(CursorLockMode.None, true);
            }
            else
            {
                pauseMenuPanel.SetActive(false);
                TerminatePausedState();
                SetCursorState(CursorLockMode.Locked, false);
            }

        }
        private void StartPausedState()
        {
            playerDataMono.playerState = PlayerState.Paused;
            playerDataMono.playerStateStack.Add(playerDataMono.playerState);
        }
        private void TerminatePausedState()
        {
            playerDataMono.playerStateStack.Remove(PlayerState.Paused);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
        }
        private void SetCursorState(CursorLockMode cursorLockMode, bool cursorVisibility)
        {
            Cursor.lockState = cursorLockMode;
            Cursor.visible = cursorVisibility;
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
                //do not destroy first two row, they are title and attribute names
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