using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player;
using Item.Interract;

namespace Item
{
    public class ShootManager : NetworkBehaviour
    {
        //Network Properties
        [Networked, OnChangedRender(nameof(OnFireChanged))]
        public bool isFiring { get; set; }

        [Header("References")]
        [SerializeField] InterractComponent interractComponent;

        [Header("Fire Datas")]
        [SerializeField] private WeaponDataMono weaponDataMono;
        public Transform aimPoint;
        float lastTimeFired = 0;

        [Header("Recoil Follow")]
        private int currentRecoilIndex = 1;
        private Vector2 currentRecoil;
        private Transform cameraTransform;
        private bool isRecoiling = false;

        [Header("Effects")]
        public ParticleSystem fireParticleSystem;

        public override void Spawned()
        {
            
        }

        #region SYNC_FIRE
        public void OnFireChanged()
        {
            if (isFiring)
                OnFireRemote();

        }
        void OnFireRemote()
        {
            if (!Object.HasInputAuthority)
                fireParticleSystem.Play();
        }
        #endregion

        #region FIRE_ACTION
        public void Fire(Vector3 aimForwardVector)
        {
            //Limit fire rate
            if (Time.time - lastTimeFired < 0.15f)
                return;

            interractComponent.Owner.gameObject.TryGetComponent<PlayerDataMono>(out PlayerDataMono playerData);
            if (weaponDataMono.ammo <= 0 && playerData != null)
            {
                if(playerData.playerState != PlayerState.Reloading && weaponDataMono.fullAmmo > 0)
                    StartCoroutine(Reload());

                return;
            }

            StartCoroutine(FireEffectCO());
                

            //recoil aplied
            if(Object.HasInputAuthority)
                ApplyRecoil();

            float hitDistance = weaponDataMono.weaponShootSettings.hitDistance;

            Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, hitDistance, Object.InputAuthority,
                out var hitinfo, weaponDataMono.weaponShootSettings.collisionLayers, HitOptions.IncludePhysX);

            //player hit
            if (hitinfo.Hitbox != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

                if(Object.HasStateAuthority)
                    hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamageRpc(Object.InputAuthority,
                        transform.root.root.root.gameObject.GetComponent<NetworkObject>().Id,
                        weaponDataMono.weaponShootSettings.hitDamage);

            }
            //something else hit
            else if (hitinfo.Collider != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");
            }

            lastTimeFired = Time.time;
        }

        IEnumerator FireEffectCO()
        {
            isFiring = true;
            weaponDataMono.ammo--;
            fireParticleSystem.Play();
            yield return new WaitForSeconds(0.09f);
            isFiring = false;
        }
        #endregion

        #region RELOAD
        private IEnumerator Reload()
        {
            PlayerDataMono playerDataMono = null;

            //assign player data
            interractComponent.Owner.gameObject.TryGetComponent<PlayerDataMono>(out PlayerDataMono playerData);
            if(playerData != null)
                playerDataMono = playerData;

            //set player state
            if (playerDataMono != null)
            {
                playerDataMono.playerState = PlayerState.Reloading;
                playerDataMono.playerStateStack.Add(PlayerState.Reloading);
            }

            //wait 2 seconds for reloading...
            yield return new WaitForSeconds(2f);

            //set ammo values
            weaponDataMono.ammo = weaponDataMono.fullAmmo <= weaponDataMono.weaponDataSettings.ammo ?
                weaponDataMono.fullAmmo : weaponDataMono.weaponDataSettings.ammo;

            weaponDataMono.fullAmmo = weaponDataMono.fullAmmo <= weaponDataMono.ammo ?
                0 : weaponDataMono.fullAmmo - weaponDataMono.ammo;

            //set player state to previous
            if (playerDataMono != null)
            {
                playerDataMono.playerStateStack.Remove(PlayerState.Reloading);
                playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
            }

        }
        #endregion

        #region FIRE_RECOIL
        public void ApplyRecoil()
        {
            if (weaponDataMono.weaponShootSettings.recoilPattern == null || weaponDataMono.weaponShootSettings.recoilPattern.pattern.Length == 0)
                return;

            cameraTransform = Player.NetworkPlayer.Local.gameObject.GetComponentInChildren<Camera>().transform;

            // Get the current recoil pattern point
            Vector2 recoilPoint = weaponDataMono.weaponShootSettings.recoilPattern.pattern[currentRecoilIndex];

            // Add random tolerance
            float randomX = Random.Range(-weaponDataMono.weaponShootSettings.recoilTolerance, weaponDataMono.weaponShootSettings.recoilTolerance);
            float randomY = Random.Range(-weaponDataMono.weaponShootSettings.recoilTolerance, weaponDataMono.weaponShootSettings.recoilTolerance);

            currentRecoil = new Vector2(recoilPoint.x + randomX, recoilPoint.y + randomY) * weaponDataMono.weaponShootSettings.recoilIntensity;

            // Apply recoil movement to the weapon (rotate or move)
            StartCoroutine(ApplyRecoilToWeapon());

            // Move to the next recoil point
            currentRecoilIndex++;

            if (currentRecoilIndex >= weaponDataMono.weaponShootSettings.recoilPattern.pattern.Length)
            {
                currentRecoilIndex--; // Loop the pattern if necessary
            }
        }
        IEnumerator ApplyRecoilToWeapon()
        {
            isRecoiling = true;
            yield return new WaitForSeconds(0.2f);
            isRecoiling = false;

            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (isRecoiling)
                    yield break;
            }
            currentRecoilIndex = 1;
        }
        public override void FixedUpdateNetwork()
        {
            if(isRecoiling)
            {
                cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation,
                    Quaternion.Euler(new Vector3(currentRecoil.y, currentRecoil.x, 0)),
                    weaponDataMono.weaponShootSettings.recoilSpeed);
            }
            else if(cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation,
                    Quaternion.Euler(Vector3.zero),
                    weaponDataMono.weaponShootSettings.recoilSpeed);
            }

            if(GetComponent<InterractComponent>().isItemActive && Object.HasInputAuthority)
            {
                if(Player.NetworkPlayer.Local)
                    Player.NetworkPlayer.Local.GetComponent<CharacterInputHandler>().isAuto = weaponDataMono.weaponShootSettings.isAuto;
            }
        }
        #endregion



    }
}

