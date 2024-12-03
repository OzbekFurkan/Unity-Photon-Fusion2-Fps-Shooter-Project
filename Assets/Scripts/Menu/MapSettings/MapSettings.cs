using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menu
{
    public enum Map { IceWorld, Skybox };

    [CreateAssetMenu(menuName = "RAW/Menu/MapSettings")]
    public class MapSettings : ScriptableObject
    {
        public Map mapId;
        public Sprite mapIcon;
        public string mapName;
    }
}