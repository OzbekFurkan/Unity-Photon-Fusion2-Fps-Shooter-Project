using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player;
using Item.Interract;
using Player.Utils;

namespace Item
{
    public class ShootManager : NetworkBehaviour, IShootable
    {
        private struct ProjectileData : INetworkStruct
        {
            public Vector3 HitPosition;
            public Vector3 HitNormal;
        }

        [Header("References")]
        [SerializeField] private InterractComponent interractComponent;
        [SerializeField] private WeaponDataMono weaponDataMono;
        [SerializeField] private ItemInput itemInput;
        [SerializeField] private Transform aimPoint;
        [SerializeField] private Animator animator;

        [Header("Fire Datas")]
        float lastTimeFired = 0;
        [Networked] Vector3 fireHitPoint { get; set; }
        [Networked] Vector3 fireHitPointNormal { get; set; }

        [Header("Recoil Follow")]
        private int currentRecoilIndex = 0;

        [Header("Effects")]
        int visibleFireCount = 0;
        [Networked] int fireCount { get; set; }
        public ParticleSystem fireParticleSystem;
        [Networked, Capacity(32)]
        private NetworkArray<ProjectileData> _projectileData { get; }

        public override void Spawned()
        {
            visibleFireCount = fireCount;
        }

        #region FIRE_ACTION
        public void Fire(Vector3 aimVector)
        {
            if (CheckShootAvailability() == false) return;

            //get player reference getter to get player camera
            interractComponent.Owner.gameObject.TryGetComponent<PlayerReferenceGetter>
                (out PlayerReferenceGetter playerReferenceGetter);

            if (playerReferenceGetter == null) return;

            Transform playerCamera = playerReferenceGetter.GetPlayerCameraHandle();

            //raycast parameters assigned
            float hitDistance = weaponDataMono.weaponShootSettings.hitDistance;
            LayerMask layerMask = weaponDataMono.weaponShootSettings.collisionLayers;
            HitOptions hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            
            //player camera raycast
            bool isHit = Runner.LagCompensation.Raycast(playerCamera.position, aimVector, hitDistance, Object.InputAuthority,
                out var detectedInfo, layerMask, hitOptions, QueryTriggerInteraction.Ignore);

            Vector3 playercameraHitpoint;
            float hitPointDistance;

            //if nothing is hit
            if (!isHit)
            {
                playercameraHitpoint = playerCamera.position+aimVector*hitDistance;
                hitPointDistance = hitDistance;
            }
            else
            {
                //player camera straight raycast results are stored in the variables below
                playercameraHitpoint = detectedInfo.Point;
                hitPointDistance = detectedInfo.Distance;
            } 
            
            //get current recoil vector to be used in shoot vector calculations
            Vector2 currentRecoil = GetCurrentRecoilVector();
            
            //recoil and distance factor added shoot vector is calculated based on player camera's raycast results

            //distace refactor to increase pattern scale in long distance
            //closer to 0 for shorter distances, closer to 0.5 for longer distances
            float distanceRefactor = Mathf.Clamp(hitPointDistance / hitDistance, 0.01f, 0.5f);

            //recoil offset vector is calculated to be added on the player camera shoot vector
            Vector3 recoilOffset = (playerCamera.right * currentRecoil.x + Quaternion.AngleAxis(playerCamera.transform.eulerAngles.x, playerCamera.right) * (-playerCamera.up * currentRecoil.y));

            Vector3 dirToHit = playercameraHitpoint - aimPoint.position;
            //final shoot vector calculated with recoil offset and distance factor, then normalized
            Vector3 shootvector = (dirToHit + recoilOffset * distanceRefactor).normalized;

            //weapon raycast (final raycast)
            Runner.LagCompensation.Raycast(aimPoint.position, shootvector, hitDistance, Object.InputAuthority,
                out var hitinfo, layerMask, hitOptions, QueryTriggerInteraction.Ignore);

            fireHitPoint = aimPoint.position + shootvector * hitDistance;
            fireHitPointNormal = Vector3.zero;

            //player hit
            if (hitinfo.Hitbox != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

                //hit point info stored in networked variables so we can use them in Render to sync hit effect
                fireHitPoint = hitinfo.Point;
                fireHitPointNormal = hitinfo.Normal;

                //hit damage
                if (Object.HasStateAuthority)
                    hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(interractComponent.Owner.Id,
                                                                                weaponDataMono.weaponShootSettings.hitDamage);

            }
            //something else hit
            else if (hitinfo.Collider != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");

                //hit point info stored in networked variables so we can use them in Render to sync hit effect
                fireHitPoint = hitinfo.Point;
                fireHitPointNormal = hitinfo.Normal;
            }

            //store bullet data so we can access hit points while continuing to fire
            //otherwise, we may lose bullet data if we shoot again before the shot is completed
            _projectileData.Set(fireCount % _projectileData.Length, new ProjectileData()
            {
                HitPosition = fireHitPoint,
                HitNormal = fireHitPointNormal
            });

            //ammo must be reduced only once by host
            if(Object.HasStateAuthority)
            {
                //ammo decrement
                weaponDataMono.ammo--;

                //fire count incerement, we will compare it with the local one and show fire effect once for each player in Render
                fireCount++;
            }

            //for pistol (non-auto)
            if (weaponDataMono.itemDataSettings.itemSlot == ItemSlot.Pistol)
                animator.SetTrigger("shooting");

        }

        private bool CheckShootAvailability()
        {
            //Limit fire rate
            if (Time.time - lastTimeFired < 0.15f) return false;

            //reset recoil index if 0.5 second passed since last time fired
            else if (Time.time - lastTimeFired > 0.5f)
                currentRecoilIndex = 0;

            //reset last time fired
            lastTimeFired = Time.time;

            //owner null check
            if (interractComponent.Owner == null) return false;

            //get player data
            interractComponent.Owner.gameObject.TryGetComponent<PlayerDataMono>(out PlayerDataMono playerData);

            //player data null check
            if (playerData == null) return false;

            //reload check
            if (weaponDataMono.ammo <= 0)
            {
                //reload if not already reloading and have magazin to reload
                if (playerData.playerState != PlayerState.Reloading && weaponDataMono.fullAmmo > 0)
                    StartCoroutine(Reload(playerData));

                return false;//return if already reloading or after reload or not have a magazin to reload
            }

            return true;
        }

        public override void Render()
        {
            ShowFireEffects();
        }

        private void ShowFireEffects()
        {
            if (weaponDataMono.itemDataSettings.itemSlot == ItemSlot.Rifle)
                animator.SetBool("shooting", itemInput.isFiring);

            if (visibleFireCount < fireCount)
            {
                fireParticleSystem.Play();
            }
            
            for (int i = visibleFireCount; i < fireCount; i++)
            {
                var data = _projectileData[i % _projectileData.Length];
                
                // Show projectile visuals (e.g. spawn dummy flying projectile or trail
                // from fireTransform to data.HitPosition or spawn impact effect on data.HitPosition)
                GameObject trailGO = Instantiate(weaponDataMono.weaponShootSettings.ammoTrail, aimPoint.position, Quaternion.identity);
                trailGO.TryGetComponent<TrailRenderer>(out TrailRenderer trail);
                if (trail)
                {
                    StartCoroutine(AnimateAmmoTrailToHitPoint(trail, trail.transform.position, data.HitPosition, data.HitNormal, trail.time));
                }
            }

            visibleFireCount = fireCount;
        }
        private IEnumerator AnimateAmmoTrailToHitPoint(TrailRenderer trail, Vector3 startPos, Vector3 hitPos, Vector3 hitNormal, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Calculate t as the ratio of elapsed time to the total duration
                float t = elapsed / duration;
                trail.transform.position = Vector3.Lerp(startPos, hitPos, t);
                elapsed += Time.deltaTime;
                yield return null; // wait until next frame
            }

            // Ensure the trail ends exactly at the hit point
            trail.transform.position = hitPos;
            Destroy(trail.gameObject);

            //impact effect
            if (hitPos != Vector3.zero)
            {
                GameObject impactEffect = Instantiate(weaponDataMono.weaponShootSettings.impactEffectPrefab,
                    hitPos + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal));
                Destroy(impactEffect, 2f);
            }
        }
        #endregion

        #region RELOAD
        private IEnumerator Reload(PlayerDataMono playerDataMono)
        {
            //set player state
            playerDataMono.playerState = PlayerState.Reloading;
            playerDataMono.playerStateStack.Add(PlayerState.Reloading);

            animator.SetTrigger("reloading");

            //wait 2 seconds for reloading...
            yield return new WaitForSeconds(2f);

            //set ammo
            weaponDataMono.ammo = weaponDataMono.fullAmmo <= weaponDataMono.weaponDataSettings.ammo ?
                weaponDataMono.fullAmmo : weaponDataMono.weaponDataSettings.ammo;

            //set full ammo
            weaponDataMono.fullAmmo = weaponDataMono.fullAmmo <= weaponDataMono.ammo ?
                0 : weaponDataMono.fullAmmo - weaponDataMono.ammo;

            //set player state to previous
            playerDataMono.playerStateStack.Remove(PlayerState.Reloading);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();

        }
        #endregion

        #region FIRE_RECOIL
        public Vector2 GetCurrentRecoilVector()
        {
            RecoilPatternData recoilPattern = weaponDataMono.weaponShootSettings.recoilPattern;
            int patternLength = weaponDataMono.weaponShootSettings.recoilPattern.pattern.Length;

            if (recoilPattern == null || patternLength == 0)
                return Vector2.zero;

            float recoilTolerance = weaponDataMono.weaponShootSettings.recoilTolerance;
            float recoilIntensity = weaponDataMono.weaponShootSettings.recoilIntensity;

            // Get the current recoil pattern point and incerement index of pattern
            Vector2 recoilPoint = recoilPattern.pattern[currentRecoilIndex++];

            //move to previous vector if pattern length is exceeded
            if (currentRecoilIndex >= patternLength)
            {
                currentRecoilIndex--;
                recoilTolerance *= 5;
            }

            // Add random tolerance
            float randomX = Random.Range(-recoilTolerance, recoilTolerance);
            float randomY = Random.Range(-recoilTolerance, recoilTolerance);

            return new Vector2(recoilPoint.x + randomX, recoilPoint.y + randomY) * recoilIntensity;
        }
        #endregion



    }
}

