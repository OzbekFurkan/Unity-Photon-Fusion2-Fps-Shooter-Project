using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(menuName = "RAW/Shoot/ShootSettings")]
    public class WeaponShootSettings : ScriptableObject
    {

        [Header("Shoot")]
        public LayerMask collisionLayers;
        public float hitDistance;
        public byte hitDamage;
        public GameObject impactEffectPrefab;
        public bool isAuto;

        [Header("Recoil")]
        [Tooltip("draw a  pattern in Window -> Recoil Pattern Drawer. it will be saved into Scripts -> Item -> Weapon -> " +
            "Weapon -> Shoot -> Shoot Settings -> Recoil Pattern")]
        public RecoilPatternData recoilPattern;
        [Range(0,1)]
        public float recoilIntensity;
        [Range(0, 1)]
        public float raycastRecoilIntensity;
        public float recoilTolerance; // Random tolerance for weapon inaccuracy
        [Range(0,1)]
        public float recoilSpeed;

    }
}

