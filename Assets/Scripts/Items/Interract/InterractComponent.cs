using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player;
using Player.Interract;

namespace Item.Interract
{
    public class InterractComponent : NetworkBehaviour
    {
        [Header("Network Related Variables")]
        private GameObject parentPlayerObject;
        [Networked, HideInInspector] public bool IsPickedUp { get; set; }
        [HideInInspector][Networked] public NetworkObject Owner { get; set; }
        [Networked, HideInInspector, OnChangedRender(nameof(OnRigChange))] public bool isKinematicRig { get; set; }
        [Networked, HideInInspector, OnChangedRender(nameof(OnColChange))] public bool isTriggerCol { get; set; }
        [Networked, HideInInspector] public bool isItemActive { get; set; }//is it our current item

        [Header("Components")]
        [SerializeField] Rigidbody itemRigidbody;
        [SerializeField] BoxCollider boxCollider;
        public GameObject itemUI;

        //Mutex Flags
        [Networked, HideInInspector] public NetworkBool isPickupComplete { get; set; }
        [Networked, HideInInspector] public NetworkBool isDropComplete { get; set; }


        #region NETWORK_SYNC
        public void OnRigChange()
        {
             Debug.Log("iskinematic: " + isKinematicRig);
             itemRigidbody.isKinematic = isKinematicRig;
        }
        public void OnColChange()
        {
            Debug.Log("iscollider: " + isTriggerCol);
            boxCollider.isTrigger = isTriggerCol;
        }
        public override void Spawned()
        {
            
            isPickupComplete = true;
            isDropComplete = true;
        }
        #endregion

        #region PICKUP_ITEM
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void PickUpItemRpc(PlayerRef newOwner, NetworkId parentNetworkObjectid)
        {
            if (IsPickedUp || !isPickupComplete)
                return;

            isPickupComplete = false;
            SetItemProps(newOwner, parentNetworkObjectid);
            SetWeaponTransformData(parentPlayerObject);
            ToggleColliderIsTrigger(true);
            SendPickUpCallBack();
            isPickupComplete = true;
        }
        private void SetItemProps(PlayerRef newOwner, NetworkId parentNetworkObjectid)
        {
            IsPickedUp = true;
            itemRigidbody.isKinematic = true;
            isKinematicRig = true;
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
            PIM.SendPickUpCallBackRpc(itemDataMono.itemId, Object.Id);
        }

        private void SetWeaponTransformData(GameObject parent)
        {
            ItemDataMono itemDataMono = GetComponent<ItemDataMono>();
            Transform WeaponHolder = parent.transform.GetChild(1).GetChild(0).GetChild(1);
            Debug.Log(parent.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(itemDataMono.itemSlot).gameObject.name);
            parent.GetComponent<ItemSwitch>().SwitchSlot(itemDataMono.itemSlot);//change slot to which possessed item's slot is
            transform.SetParent(WeaponHolder.GetChild(itemDataMono.itemSlot));
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        #endregion

        #region DROP_ITEM
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void DropItemRpc()
        {
            if (!IsPickedUp || !isItemActive || !isDropComplete)
                return;

            isDropComplete = false;
            ResetItemProps();
            ThrowWeapon();
            ToggleColliderIsTrigger(false);
            SendDropCallBack();
            parentPlayerObject = null;
            isDropComplete = true;
        }
        public void ResetItemProps()
        {
            IsPickedUp = false;
            Owner = null;
            transform.SetParent(null);
            isItemActive = false;
            itemRigidbody.isKinematic = false;
            isKinematicRig = false;
            Object.RemoveInputAuthority();
        }
        private void ThrowWeapon()
        {
            itemRigidbody.AddForce(transform.forward, ForceMode.Impulse);
        }
        private void SendDropCallBack()
        {
            PlayerInterractManager PIM = parentPlayerObject.GetComponent<PlayerInterractManager>();
            ItemDataMono itemDataMono = GetComponent<ItemDataMono>();
            PIM.SendDropCallBackRpc(itemDataMono.itemId);
        }
        #endregion

        private void ToggleColliderIsTrigger(bool current)
        {
            boxCollider.isTrigger = current;
            isTriggerCol = current;
        }

        public GameObject GetItemUI()
        {
            return itemUI;
        }

    }
}

