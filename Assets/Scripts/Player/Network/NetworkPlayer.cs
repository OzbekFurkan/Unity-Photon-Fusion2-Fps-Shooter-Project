using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilitiy;

namespace Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        //singleton to access local player object easier
        public static NetworkPlayer Local { get; set; }

        [Header("References")]
        public Transform playerModel;
        public Transform _hitbox;
        public Camera minimapCamera;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;//assign singleton if it is us

                //Sets the layer of the local players model and our player model becomes visible for only remote players
                Utility.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                //Sets the layer of the hitbox so we can ignore our own hitbox while shooting
                Utility.SetRenderLayerInChildren(_hitbox, LayerMask.NameToLayer("Ignore Raycast"));

                Debug.Log("Spawned local player");
            }
            else
            {
                //Disable the minimap camera if we are not the local player
                minimapCamera.enabled = false;

                Debug.Log("Spawned remote player");
            }

            //Make it easier to tell which player is which.
            transform.name = $"P_{Object.InputAuthority.PlayerId}";
        }
    }
}

