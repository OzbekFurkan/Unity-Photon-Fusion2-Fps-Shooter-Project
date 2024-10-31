using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilitiy;
using InventorySpace;
using Player;
using Item;

namespace Interract
{
    public class InterractComponent : NetworkBehaviour
    {
        [Header("Network Related Variables")]
        private GameObject parentPlayerObject;
        [Networked] public bool IsPickedUp { get; set; }
        [HideInInspector][Networked] public NetworkObject Owner { get; set; }
        [Networked, OnChangedRender(nameof(OnRigChange))] public bool isKinematicRig { get; set; }
        [Networked, OnChangedRender(nameof(OnColChange))] public bool isTriggerCol { get; set; }
        [Networked] public bool isItemActive { get; set; }//is it our current item

        [Header("Components")]
        [SerializeField] Rigidbody itemRigidbody;
        [SerializeField] BoxCollider boxCollider;
        

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
            if (Object.HasInputAuthority)
            {
                InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
                foreach (InterractComponent item in allItems)
                {
                    item.OnRigChange();
                    item.OnColChange();
                }
            }
        }
        #endregion

        #region PICKUP_ITEM
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void PickUpItemRpc(PlayerRef newOwner, NetworkId parentNetworkObjectid)
        {
            if (IsPickedUp)
                return;

            SetItemProps(newOwner, parentNetworkObjectid);
            SetWeaponTransformData(parentPlayerObject);
            ToggleColliderIsTrigger(true);
            SendPickUpCallBack();
            
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
            Type type = itemDataMono.GetType();
            MethodInfo methodInfo = typeof(PlayerInterractManager).GetMethod("SendPickUpCallBack").MakeGenericMethod(type);
            methodInfo.Invoke(PIM, new object[] { Object.Id });
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
            if (!IsPickedUp || !isItemActive)
                return;

            ResetItemProps();
            ThrowWeapon();
            ToggleColliderIsTrigger(false);
            SendDropCallBack();
            parentPlayerObject = null;
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
            Type type = itemDataMono.GetType();
            MethodInfo methodInfo = typeof(PlayerInterractManager).GetMethod("SendDropCallBack").MakeGenericMethod(type);
            methodInfo.Invoke(PIM, new object[] { Object.Id });
        }
        #endregion

        private void ToggleColliderIsTrigger(bool current)
        {
            boxCollider.isTrigger = current;
            isTriggerCol = current;
        }

    }
}

