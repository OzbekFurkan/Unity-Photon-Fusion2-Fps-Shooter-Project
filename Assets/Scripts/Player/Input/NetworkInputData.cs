using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Player
{
    public struct NetworkInputData : INetworkInput
    {
        //movement
        public Vector2 movementInput;
        public Vector3 aimForwardVector;
        public Vector2 lookRotationVector;
        public NetworkButtons Buttons;
    }
}
