using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using GameModes.Modes;

namespace Player
{
    public enum InputButton
    {
        Jump,
        Fire,
        Drop,
        PickUp,
        SlotRiffle,
        SlotPistol,
        SlotKnife,
        SlotBomb,
        SlotOther,
        Leaderboard,
        Pause
    }

    public class CharacterInputHandler : NetworkBehaviour, IBeforeUpdate, IAfterTick
    {
        [Header("References")]
        [SerializeField] private InputSettings inputSettings;
        [SerializeField] private PlayerDataMono playerDataMono;
        private BaseGameMode baseGameMode;

        [Networked]
        public NetworkButtons PreviousButtons { get; private set; }
        public Vector2 lookRotation => _networkInput.lookRotationVector;

        private NetworkInputData _networkInput;

        //this variable assigned in ShootManager and decides whether the shooting proccess will be in auto mode ore not
        public bool isAuto;

        //PlayerUI
        //Needed for ui implementation in common ui (PlayerUIController)
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

        public override void Spawned()
        {
            if (HasInputAuthority == false)
                return;

            // Register to Fusion input poll callback
            var networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (runner == null)
                return;

            var networkEvents = runner.GetComponent<NetworkEvents>();
            if (networkEvents != null)
            {
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }

        public void BeforeUpdate()
        {
            if (HasInputAuthority == false)
                return;

            // Accumulate input only if the cursor is locked.
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                _networkInput.movementInput = default;
                return;
            }

            //player ui

            //leaderboard
            if (Input.GetKeyDown(inputSettings.leaderboardKey))
                isLeaderboardButtonPressed = true;
            else if (Input.GetKeyUp(inputSettings.leaderboardKey))
                isLeaderboardButtonPressed = false;
            _networkInput.Buttons.Set(InputButton.Leaderboard, isLeaderboardButtonPressed);

            //pause
            if (Input.GetKeyDown(inputSettings.pauseKey))
                isPauseButtonPressed = !isPauseButtonPressed;
            _networkInput.Buttons.Set(InputButton.Pause, isPauseButtonPressed);

            //no movement or action allowed when paused
            if (playerDataMono.playerState == PlayerState.Paused)
                return;

            //mouse look
            _networkInput.lookRotationVector += new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
            localCameraHandler.SetViewInputVector(_networkInput.lookRotationVector);

            //movement
            var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _networkInput.movementInput = moveDirection.normalized;

            //jump
            _networkInput.Buttons.Set(InputButton.Jump, Input.GetKey(inputSettings.jumpKey));

            //fire
             _networkInput.Buttons.Set(InputButton.Fire, Input.GetKey(inputSettings.shootKey));

            //interract
            //can not intract while reloading or already interacting (other possibilities already eliminated above and OnInput)
            if (playerDataMono.playerState == PlayerState.Playing)
            {
                _networkInput.Buttons.Set(InputButton.PickUp, Input.GetKey(inputSettings.pickupKey));
                _networkInput.Buttons.Set(InputButton.Drop, Input.GetKey(inputSettings.dropkey));
            }

            //item slot switch
            _networkInput.Buttons.Set(InputButton.SlotRiffle, Input.GetKey(inputSettings.riffleSlotKey));
            _networkInput.Buttons.Set(InputButton.SlotPistol, Input.GetKey(inputSettings.pistolSlotKey));
            _networkInput.Buttons.Set(InputButton.SlotKnife, Input.GetKey(inputSettings.knifeSlotKey));
            _networkInput.Buttons.Set(InputButton.SlotBomb, Input.GetKey(inputSettings.bombSlotKey));
            _networkInput.Buttons.Set(InputButton.SlotOther, Input.GetKey(inputSettings.otherSlotKey));
        }

        public void AfterTick()
        {
            // Save current button input as previous.
            // Previous buttons need to be networked to detect correctly pressed/released events.
            PreviousButtons = GetInput<NetworkInputData>().GetValueOrDefault().Buttons;
        }

        // Fusion polls accumulated input. This callback can be executed multiple times in a row if there is a performance spike.
        private void OnInput(NetworkRunner runner, NetworkInput networkInput)
        {
            baseGameMode = FindObjectOfType<BaseGameMode>();

            //no input allowed when player died
            if (playerDataMono.playerState != PlayerState.Died && baseGameMode != null && baseGameMode.gameState == GameState.Playing)
                networkInput.Set(_networkInput);
        }

    }
}

