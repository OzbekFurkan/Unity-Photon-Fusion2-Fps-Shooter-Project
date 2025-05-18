using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item.Interract;
using Player;

namespace Item
{
    public class BombShootManager : NetworkBehaviour, IShootable
    {
        [SerializeField] private BombDataMono bombDataMono;
        [SerializeField] private InterractComponent _interactComponent;

        int visibleFireCount = 0;
        [Networked] int fireCount { get; set; }

        private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        public void Fire(Vector3 aimVector)
        {
            //owner null check
            if (_interactComponent.Owner == null) return;

            //get player data
            _interactComponent.Owner.gameObject.TryGetComponent<PlayerDataMono>(out PlayerDataMono playerData);

            //player data null check
            if (playerData == null) return;

            NetworkId _owner = _interactComponent.Owner.Id;

            if(Object.HasStateAuthority)
                _interactComponent.DropItem(bombDataMono.bombDataSettings.throwForce);

            StartCoroutine(DelayForExplosion(_owner));

            
        }

        private IEnumerator DelayForExplosion(NetworkId _owner)
        {
            yield return new WaitForSeconds(2f);

            fireCount++;

            float _impactRadius = bombDataMono.bombDataSettings.impactRadius;
            LayerMask layerMask = bombDataMono.bombDataSettings.layerMask;
            HitOptions hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

            Runner.LagCompensation.OverlapSphere(transform.position, _impactRadius, Object.InputAuthority,
                hits, layerMask, hitOptions);

            if (hits == null) yield break;

            foreach (LagCompensatedHit hit in hits)
            {
                //hit non-player object
                if (hit.Collider != null)
                {
                    Debug.Log("normal object: "+hit.GameObject.name);
                    continue;
                }
                //player hit
                else if(hit.Hitbox != null)
                {
                    Debug.Log("player object: " + hit.GameObject.name);

                    hit.Hitbox.transform.root.gameObject.TryGetComponent<HPHandler>(out HPHandler hPHandler);
                    if (hPHandler == null) continue;

                    if (Object.HasStateAuthority)
                        hPHandler.OnTakeDamage(_owner, bombDataMono.bombDataSettings.hitDamage);
                }
                
            }

        }

        public override void Render()
        {
            if(fireCount > visibleFireCount)
            {
                ParticleSystem particle = Instantiate(bombDataMono.bombDataSettings.explosionParticle,
                    transform.position, transform.rotation);

                Destroy(particle.gameObject, bombDataMono.bombDataSettings.explosionParticle.main.duration);

                if(Object.HasStateAuthority)
                {
                    Runner.Despawn(Object);
                    return;
                }
                    
            }

            visibleFireCount = fireCount;

        }

    }
}

