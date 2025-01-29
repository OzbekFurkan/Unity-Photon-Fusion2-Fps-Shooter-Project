using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Utilitiy;
using GameModes.Common;

namespace Player
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        bool isRespawnRequested = false;

        [Header("Movement Setup")]
        public float JumpImpulse = 5f;
        public float MovementSpeed = 8.0f;

        [Header("Player References")]
        [SerializeField] SimpleKCC KCC;
        [SerializeField] HPHandler hpHandler;
        [SerializeField] PlayerDataMono playerData;
        [SerializeField] Transform CameraPivot;
        [SerializeField] CharacterInputHandler _input;

        public override void Spawned()
        {
            KCC.SetGravity(-9.81f * 2);
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                if (isRespawnRequested)
                {
                    Respawn();
                    return;
                }

                //Don't update the clients position when they are dead
                if (hpHandler.isDead)
                    return;
            }

            //get the input from network
            var input = GetInput<NetworkInputData>();
            ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);

        }

        private void ProcessInput(NetworkInputData input, NetworkButtons previousButtons)
        {
            KCC.SetLookRotation(input.lookRotationVector, -90f, 90f);

            //Move Vector
            Vector3 moveDirection = transform.forward * input.movementInput.y + transform.right * input.movementInput.x;
            moveDirection.Normalize();

            float jumpImpulse = 0.0f;

            //Jump
            if (input.Buttons.WasPressed(previousButtons, InputButton.Jump) && KCC.IsGrounded)
            {
                jumpImpulse = JumpImpulse;
            }

            KCC.Move(moveDirection * MovementSpeed, jumpImpulse);

            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

        void CheckFallRespawn()
        {
            if (transform.position.y < -12)
            {
                if (Object.HasStateAuthority)
                {
                    Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                    Respawn();
                }

            }
        }

        public void RequestRespawn()
        {
            isRespawnRequested = true;
        }

        void Respawn()
        {
            SpawnHandler _spawnHandler = FindObjectOfType<SpawnHandler>();
            if(_spawnHandler != null)
                KCC.Move((playerData.team == Team.Soldier)?
                    _spawnHandler.soldierSpawnPointContainer.GetChild(_spawnHandler.soldierSpawnPointContainer.childCount-1).position:
                    _spawnHandler.alienSpawnPointContainer.GetChild(_spawnHandler.alienSpawnPointContainer.childCount - 1).position);

            hpHandler.OnRespawned();

            isRespawnRequested = false;
        }

        public void SetCharacterControllerEnabled(bool isEnabled)
        {
            KCC.SetActive(isEnabled);
        }

    }
}

