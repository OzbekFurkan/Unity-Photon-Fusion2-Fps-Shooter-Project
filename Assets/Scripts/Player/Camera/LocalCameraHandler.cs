using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Player
{
    public class LocalCameraHandler : NetworkBehaviour
    {

        //Input
        Vector2 viewInput;

        //Rotation
        float cameraRotationX = 0;

        float cameraRotationY = 0;

        public float viewUpDownRotationSpeed;
        public float rotationSpeed;
        public float clampValue;

        Camera localCamera;

        private void Awake()
        {
            localCamera = GetComponentInChildren<Camera>();
        }

        public override void FixedUpdateNetwork()
        {
            
        }

        private void LateUpdate()
        {
            if (!localCamera.enabled)
                return;

            //Calculate rotation
            cameraRotationX += viewInput.y * Time.deltaTime * viewUpDownRotationSpeed;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -clampValue, clampValue);

            cameraRotationY += viewInput.x * Time.deltaTime * rotationSpeed;

            // Apply rotation locally
            transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);

        }

        public void SetViewInputVector(Vector2 viewInput)
        {
            this.viewInput = viewInput;
        }
    }
}

