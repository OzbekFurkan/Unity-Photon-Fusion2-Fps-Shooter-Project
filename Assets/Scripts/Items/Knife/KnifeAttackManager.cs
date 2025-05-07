using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item.Interract;
using Player;
using Player.Utils;

namespace Item
{
    public class KnifeAttackManager : NetworkBehaviour, IShootable
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private KnifeDataMono knifeDataMono;
        [SerializeField] private InterractComponent interractComponent;

        public void Fire(Vector3 aimVector)
        {
            //owner null check
            if (interractComponent.Owner == null) return;

            //get player data
            interractComponent.Owner.gameObject.TryGetComponent<PlayerDataMono>(out PlayerDataMono playerData);

            //player data null check
            if (playerData == null) return;

            //get player reference getter to get player camera
            interractComponent.Owner.gameObject.TryGetComponent<PlayerReferenceGetter>
                (out PlayerReferenceGetter playerReferenceGetter);

            if (playerReferenceGetter == null) return;

            Transform playerCamera = playerReferenceGetter.GetPlayerCameraHandle();

            if (playerCamera == null) return;

            //raycast parameters assigned
            float hitDistance = knifeDataMono.knifeDataSettings.hitDistance;
            LayerMask layerMask = knifeDataMono.knifeDataSettings.layerMask;
            HitOptions hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

            //player camera raycast
            bool isHit = Runner.LagCompensation.Raycast(playerCamera.position, aimVector, hitDistance, Object.InputAuthority,
                out var detectedInfo, layerMask, hitOptions, QueryTriggerInteraction.Ignore);
            
            //if nothing is hit
            if (!isHit) return;
            
            //player hit
            if(detectedInfo.Hitbox != null)
            {
                detectedInfo.Hitbox.transform.root.gameObject.TryGetComponent<HPHandler>(out HPHandler hPHandler);
                if (hPHandler == null) return;
                
                if (Object.HasStateAuthority)
                    hPHandler.OnTakeDamage(interractComponent.Owner.Id, knifeDataMono.knifeDataSettings.hitDamage);
            }
            //something else hit
            else if(detectedInfo.Collider != null)
            {
                
            }

        }

        
    }
}

