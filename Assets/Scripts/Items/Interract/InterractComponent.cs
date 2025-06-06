using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilitiy;
using Player;
using Player.Interract;
using Player.Utils;

namespace Item.Interract
{
    public class InterractComponent : NetworkBehaviour
    {
        [Header("Network Related Variables")]
        private GameObject parentPlayerObject;
        [Networked, HideInInspector, OnChangedRender(nameof(OnPickUpStateChange))] public bool IsPickedUp { get; set; }
        [HideInInspector][Networked] public NetworkObject Owner { get; set; }
        [Networked, HideInInspector] public bool isKinematicRig { get; set; }
        [Networked, HideInInspector] public bool isTriggerCol { get; set; }
        [Networked, HideInInspector] public bool isItemActive { get; set; }//is it our current item

        [Header("Components")]
        [SerializeField] Rigidbody itemRigidbody;
        [SerializeField] Collider _collider;

        //Mutex Flags
        [Networked, HideInInspector] public NetworkBool isPickupComplete { get; set; }
        [Networked, HideInInspector] public NetworkBool isDropComplete { get; set; }


        #region NETWORK_SYNC
        public void OnPickUpStateChange()
        {
            itemRigidbody.isKinematic = isKinematicRig;
            _collider.isTrigger = isTriggerCol;
        }
        public override void Spawned()
        {
            isPickupComplete = true;
            isDropComplete = true;
        }
        #endregion

        #region PICKUP_ITEM
        public void PickUpItem(PlayerRef newOwner, NetworkId parentNetworkObjectid)
        {
            if (IsPickedUp || !isPickupComplete)
                return;

            isPickupComplete = false;
            SetItemProps(newOwner, parentNetworkObjectid);
            SetWeaponTransformData(parentPlayerObject);
            SendPickUpCallBack();
            isPickupComplete = true;
        }
        private void SetItemProps(PlayerRef newOwner, NetworkId parentNetworkObjectid)
        {
            IsPickedUp = true;
            ToggleRigidbodyIsKinematic(true);
            ToggleColliderIsTrigger(true);
            isItemActive = true;
            parentPlayerObject = Runner.FindObject(parentNetworkObjectid).gameObject;
            Owner = parentPlayerObject.GetComponent<NetworkObject>();
            Object.AssignInputAuthority(newOwner);
        }
        private void SendPickUpCallBack()
        {
            Debug.Log("pickup callback yolancak");
            PlayerInterractManager PIM = parentPlayerObject.GetComponent<PlayerInterractManager>();
            ItemDataMono itemDataMono = GetComponent<ItemDataMono>();
            PIM.SendPickUpCallBack((int)itemDataMono.itemDataSettings.itemId, Object.Id);
        }

        private void SetWeaponTransformData(GameObject parent)
        {
            ItemDataMono itemDataMono = GetComponent<ItemDataMono>();
            parent.TryGetComponent<PlayerReferenceGetter>(out PlayerReferenceGetter playerReferenceGetter);

            if (playerReferenceGetter == null)
                return;

            Transform weaponHolder = playerReferenceGetter.GetWeaponHolder();

            parent.GetComponent<ItemSwitch>().SwitchSlot((int)itemDataMono.itemDataSettings.itemSlot);//change slot to possessed item's slot
            transform.SetParent(weaponHolder.GetChild((int)itemDataMono.itemDataSettings.itemSlot));
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        #endregion

        #region DROP_ITEM
        public void DropItem(float throwForce = 1f)
        {
            if (!IsPickedUp || !isItemActive || !isDropComplete) return;

            isDropComplete = false;
            ResetItemProps();
            ThrowItem(throwForce);
            SendDropCallBack();
            parentPlayerObject = null;
            isDropComplete = true;
        }
        public void ResetItemProps()
        {
            IsPickedUp = false;
            Owner = null;
            transform.SetParent(FindObjectOfType<ParentSyncInScene>().transform);
            isItemActive = false;
            ToggleRigidbodyIsKinematic(false);
            ToggleColliderIsTrigger(false);
            Object.RemoveInputAuthority();
        }
        private void ThrowItem(float throwForce = 1f)
        {
            itemRigidbody.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
        private void SendDropCallBack()
        {
            PlayerInterractManager PIM = parentPlayerObject.GetComponent<PlayerInterractManager>();
            ItemDataMono itemDataMono = GetComponent<ItemDataMono>();
            PIM.SendDropCallBack((int)itemDataMono.itemDataSettings.itemId);
        }
        #endregion

        private void ToggleRigidbodyIsKinematic(bool current)
        {
            itemRigidbody.isKinematic = current;
            isKinematicRig = current;
        }
        private void ToggleColliderIsTrigger(bool current)
        {
            _collider.isTrigger = current;
            isTriggerCol = current;
        }

    }
}

