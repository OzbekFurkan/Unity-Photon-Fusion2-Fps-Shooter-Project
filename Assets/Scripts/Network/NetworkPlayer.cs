using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilitiy;

namespace Network
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        public static NetworkPlayer Local { get; set; }

        public Transform playerModel;
        public Camera localCamera;
        public Camera minimapCamera;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
                
                //Sets the layer of the local players model
                Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                Debug.Log("Spawned local player");
            }
            else
            {
                //Disable the camera if we are not the local player
                localCamera.enabled = false;
                minimapCamera.enabled = false;

                //Only 1 audio listner is allowed in the scene so disable remote players audio listner
                AudioListener audioListener = GetComponentInChildren<AudioListener>();
                audioListener.enabled = false;

                Debug.Log("Spawned remote player");
            }

            //Make it easier to tell which player is which.
            transform.name = $"P_{Object.InputAuthority.PlayerId}";
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (player == Object.InputAuthority)
                Runner.Despawn(Object);

        }
    }
}

