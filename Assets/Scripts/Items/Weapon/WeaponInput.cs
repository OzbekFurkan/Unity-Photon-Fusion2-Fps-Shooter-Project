using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Network;

namespace Item
{
    public class WeaponInput : NetworkBehaviour
    {

        private ShootManager shootManager;
        private void Awake()
        {
            shootManager = GetComponent<ShootManager>();
        }
        public override void FixedUpdateNetwork()
        {

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isFireButtonPressed)
                    shootManager.Fire(networkInputData.aimForwardVector);
            }
        }


    }
}
