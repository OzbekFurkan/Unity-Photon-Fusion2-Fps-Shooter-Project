using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utilitiy
{
    [CustomEditor(typeof(ParentSyncInScene))]
    public class CustomInspectorParentSyncPlayer : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This empty class is used to sync parent of dropped items, otherwise it might crash. " +
                "Attach it to empty gameobject in the scene. All Interractable items will be its childs.", MessageType.Info);
        }
    }
    [CustomEditor(typeof(ParentSyncEmptyClass))]
    public class CustomInspectorParentSyncScene : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This empty class is used to sync parent of picked items, otherwise it won't be able to " +
                "sync player's possesed items. Attach it to each slot gameobject in the weapon holder of Player. " +
                "All Interractable items will be its childs.", MessageType.Info);
        }
    }
}