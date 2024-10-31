using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [System.Serializable]
    public class RecoilPatternData : ScriptableObject
    {
        public Vector2[] pattern;
        [HideInInspector]public Vector2[] recoilPatternForVisualization;//the points for only visualization in inspector
    }
}