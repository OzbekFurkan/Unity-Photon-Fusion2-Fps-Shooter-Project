using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public class KnifeDataMono : ItemDataMono, IHandAttachable
    {
        public KnifeDataSettings knifeDataSettings;
        [SerializeField] private ItemReferenceGetter itemReferenceGetter;

        public Transform leftHandTarget { get; set; }
        public Transform rightHandTarget { get; set; }

        private void Awake()
        {
            leftHandTarget = itemReferenceGetter.LeftHandTargetGetter();
            rightHandTarget = itemReferenceGetter.RightHandTargetGetter();
        }

        public Transform GetLeftHandTarget()
        {
            return leftHandTarget;
        }

        public Transform GetRightHandTarget()
        {
            return rightHandTarget;
        }

        public override void Spawned()
        {
            SetItemDataSettings(knifeDataSettings);
        }

        protected override void SetItemDataSettings(ItemDataSettings itemDataSettings)
        {
            base.itemDataSettings = itemDataSettings;
        }

        
    }
}

