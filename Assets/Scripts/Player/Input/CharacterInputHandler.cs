using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Player
{
    public class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private InputSettings inputSettings;
        [SerializeField] private PlayerDataMono playerDataMono;

        //movement
        Vector2 moveInputVector = Vector2.zero;
        Vector2 viewInputVector = Vector2.zero;
        [HideInInspector] bool isJumpButtonPressed = false;
        //weapon
        [HideInInspector] bool isFireButtonPressed = false;
        [HideInInspector] public bool isAuto=false;
        //interract
        [HideInInspector] bool isDropButtonPressed = false;
        [HideInInspector] bool isPickUpButtonPressed = false;
        //item slot switch
        [HideInInspector] public bool isRiffleSlotButtonPressed;
        [HideInInspector] public bool isPistolSlotButtonPressed;
        [HideInInspector] public bool isKnifeSlotButtonPressed;
        [HideInInspector] public bool isBombSlotButtonPressed;
        [HideInInspector] public bool isOtherSlotButtonPressed;
        //PlayerUI
        [HideInInspector] public bool isLeaderboardButtonPressed;
        [HideInInspector] public bool isPauseButtonPressed;

        //Other components
        LocalCameraHandler localCameraHandler;

        private void Awake()
        {
            localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            //can not set input when the game is paused by player or the player died
            if (!GetComponent<NetworkObject>().HasInputAuthority && playerDataMono.playerState == PlayerState.Died)
                return;

            //PlayerUI
            if (Input.GetKeyDown(inputSettings.leaderboardKey))
                isLeaderboardButtonPressed = true;
            else if (Input.GetKeyUp(inputSettings.leaderboardKey))
                isLeaderboardButtonPressed = false;

            if (Input.GetKeyDown(inputSettings.pauseKey))
                isPauseButtonPressed = !isPauseButtonPressed;

            if (playerDataMono.playerState == PlayerState.Paused)
                return;

            //View input
            viewInputVector.x = Input.GetAxis("Mouse X");
            viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

            //Set view
            localCameraHandler.SetViewInputVector(viewInputVector);

            //Move input
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");

            //Jump
            if (Input.GetKeyDown(inputSettings.jumpKey))
                isJumpButtonPressed = true;

            //Fire
            if (Input.GetKey(inputSettings.shootKey) && isAuto)
                isFireButtonPressed = true;
            else if(Input.GetKeyDown(inputSettings.shootKey) && !isAuto)
                isFireButtonPressed = true;

            //interract
            //can not intract while reloading or already interacting
            if(playerDataMono.playerState == PlayerState.Playing)
            {
                if (Input.GetKeyDown(inputSettings.dropkey))
                    isDropButtonPressed = true;

                if (Input.GetKeyDown(inputSettings.pickupKey))
                    isPickUpButtonPressed = true;
            }

            //item slot switch
            if (Input.GetKeyDown(inputSettings.riffleSlotKey))
                isRiffleSlotButtonPressed = true;
            if (Input.GetKeyDown(inputSettings.pistolSlotKey))
                isPistolSlotButtonPressed = true;
            if (Input.GetKeyDown(inputSettings.knifeSlotKey))
                isKnifeSlotButtonPressed = true;
            if (Input.GetKeyDown(inputSettings.bombSlotKey))
                isBombSlotButtonPressed = true;
            if (Input.GetKeyDown(inputSettings.otherSlotKey))
                isOtherSlotButtonPressed = true;

            

        }

        public NetworkInputData GetNetworkInput()
        {
            NetworkInputData networkInputData = new NetworkInputData();

            //Aim data
            networkInputData.aimForwardVector = localCameraHandler.transform.forward;

            //Move data
            networkInputData.movementInput = moveInputVector;

            //Jump data
            networkInputData.isJumpPressed = isJumpButtonPressed;

            //Fire data
            networkInputData.isFireButtonPressed = isFireButtonPressed;

            //Interract data
            networkInputData.isDropButtonPressed = isDropButtonPressed;

            networkInputData.isPickUpButtonPressed = isPickUpButtonPressed;

            //Item Slot data
            networkInputData.isRiffleSlotButtonPressed = isRiffleSlotButtonPressed;
            networkInputData.isPistolSlotButtonPressed = isPistolSlotButtonPressed;
            networkInputData.isKnifeSlotButtonPressed = isKnifeSlotButtonPressed;
            networkInputData.isBombSlotButtonPressed = isBombSlotButtonPressed;
            networkInputData.isOtherSlotButtonPressed = isOtherSlotButtonPressed;

            //playerUI
            networkInputData.isLeaderboardButtonPressed = isLeaderboardButtonPressed;
            networkInputData.isPauseButtonPressed = isPauseButtonPressed;

            //Reset variables now that we have read their states
            isJumpButtonPressed = false;
            isFireButtonPressed = false;
            isDropButtonPressed = false;
            isPickUpButtonPressed = false;
            isRiffleSlotButtonPressed = false;
            isPistolSlotButtonPressed = false;
            isKnifeSlotButtonPressed = false;
            isBombSlotButtonPressed = false;
            isOtherSlotButtonPressed = false;
            

            return networkInputData;
        }
    }
}

