using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "New Bomb", menuName = "RAW/Data/New Bomb")]
    public class BombDataSettings : ItemDataSettings
    {
        [Header("Bomb Attributes")]
        public float throwForce;
        public ParticleSystem explosionParticle;
        public float impactRadius;
        public LayerMask layerMask;
        public byte hitDamage;
        
    }
}