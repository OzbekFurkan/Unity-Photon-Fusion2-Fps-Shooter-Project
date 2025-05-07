using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "New Knife", menuName = "RAW/Data/New Knife")]
    public class KnifeDataSettings : ItemDataSettings
    {
        [Header("Knife Datas")]
        public AnimationClip knifeAnimation;
        public LayerMask layerMask;
        public float hitDistance;
        public byte hitDamage;
    }
}

