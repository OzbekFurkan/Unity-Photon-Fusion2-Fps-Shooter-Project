using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilitiy;

namespace Player
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        bool isRespawnRequested = false;

        //Other components
        NetworkCharacterController networkCharacterController;
        HPHandler hpHandler;

        private void Awake()
        {
            networkCharacterController = GetComponent<NetworkCharacterController>();
            hpHandler = GetComponent<HPHandler>();
        }

        // Start is called before the first frame update
        void Start()
        {
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

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                
                //Rotate the transform according to the client aim vector
                transform.forward = networkInputData.aimForwardVector;

                //Cancel out rotation on X axis as we don't want our character to tilt
                Quaternion rotation = transform.rotation;
                rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
                transform.rotation = rotation;

                //Move
                Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
                moveDirection.Normalize();

                networkCharacterController.Move(moveDirection);

                //Jump
                if (networkInputData.isJumpPressed)
                    networkCharacterController.Jump();

                //Check if we've fallen off the world.
                CheckFallRespawn();
            }

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
            networkCharacterController.Teleport(Utils.GetRandomSpawnPoint());

            hpHandler.OnRespawned();

            isRespawnRequested = false;
        }

        public void SetCharacterControllerEnabled(bool isEnabled)
        {
            networkCharacterController._controller.enabled = isEnabled;
        }

    }
}

