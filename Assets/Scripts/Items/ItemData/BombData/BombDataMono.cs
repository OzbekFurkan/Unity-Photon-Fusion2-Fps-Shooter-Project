using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


namespace Item
{
    public class BombDataMono : ItemDataMono, IHandAttachable
    {
        public BombDataSettings bombDataSettings;//read only
        [SerializeField] private ItemReferenceGetter itemReferenceGetter;

        public Transform leftHandTarget { get; set; }
        public Transform rightHandTarget { get; set; }

        public override void Spawned()
        {
            SetItemDataSettings(bombDataSettings);
        }

        public Transform GetLeftHandTarget()
        {
            return leftHandTarget;
        }

        public Transform GetRightHandTarget()
        {
            return rightHandTarget;
        }

        private void Start()
        {
            leftHandTarget = itemReferenceGetter.LeftHandTargetGetter();
            rightHandTarget = itemReferenceGetter.RightHandTargetGetter();
        }

        protected override void SetItemDataSettings(ItemDataSettings itemDataSettings)
        {
            base.itemDataSettings = bombDataSettings;
        }
    }
}