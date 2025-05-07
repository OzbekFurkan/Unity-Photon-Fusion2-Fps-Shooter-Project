using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;


namespace Item
{
    public class ItemReferenceGetter : MonoBehaviour
    {
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private GameObject itemUI;

        public Transform RightHandTargetGetter()
        {
            return rightHandTarget;
        }
        public Transform LeftHandTargetGetter()
        {
            return leftHandTarget;
        }
        public GameObject ItemUiGetter()
        {
            return itemUI;
        }
    }
}

