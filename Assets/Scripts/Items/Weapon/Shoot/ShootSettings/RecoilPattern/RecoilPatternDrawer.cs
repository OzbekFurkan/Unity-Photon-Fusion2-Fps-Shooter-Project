#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public class RecoilPatternDrawer : EditorWindow
    {
        private List<Vector2> recoilPattern = new List<Vector2>();
        public List<Vector2> recoilPatternForVisualization = new List<Vector2>();
        private Vector2 lastPoint = Vector2.zero;
        private bool drawingPattern = false;
        private string patternName = "NewRecoilPattern"; // Default name for the recoil pattern

        [MenuItem("Window/Recoil Pattern Drawer")]
        public static void ShowWindow()
        {
            GetWindow<RecoilPatternDrawer>("Recoil Pattern Drawer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Draw Recoil Pattern", EditorStyles.boldLabel);

            // Add input field to set the name of the recoil pattern
            patternName = EditorGUILayout.TextField("Pattern Name", patternName);

            if (GUILayout.Button("Clear Pattern"))
            {
                recoilPattern.Clear();
                Repaint();
            }

            GUILayout.Space(10);
            DrawRecoilPattern();

            GUILayout.Space(20);
            if (GUILayout.Button("Save Recoil Pattern"))
            {
                SaveRecoilPattern();
            }
        }

        private void DrawRecoilPattern()
        {
            Rect drawArea = GUILayoutUtility.GetRect(400, 400);
            EditorGUI.DrawRect(drawArea, Color.black);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && drawArea.Contains(e.mousePosition))
            {
                drawingPattern = true;
                Vector2 mousePos = e.mousePosition - drawArea.position;
                recoilPattern.Add(mousePos);
                lastPoint = mousePos;
                Repaint();
            }

            if (e.type == EventType.MouseUp)
            {
                drawingPattern = false;
            }

            Handles.color = Color.white;
            for (int i = 1; i < recoilPattern.Count; i++)
            {
                Handles.DrawLine(drawArea.position + recoilPattern[i - 1], drawArea.position + recoilPattern[i]);
            }
        }

        private void NormalizePoints()
        {
            //deep copy for visualization
            foreach(Vector2 point in recoilPattern)
            {
                recoilPatternForVisualization.Add(point);
            }
            //this part, changes all the points with their distance to the first point so the first point always will be (0,0)
            Vector2 firstPoint = recoilPattern[0];
            for(int i=0; i<recoilPattern.Count; i++)
            {
                recoilPattern[i] = recoilPattern[i] - firstPoint;
            }
            //This part, divides all the points by 10 so the values cant be too high
            for (int i = 0; i < recoilPattern.Count; i++)
            {
                recoilPattern[i] = recoilPattern[i] / 10;
            }

        }
        private void SaveRecoilPattern()
        {
            // Check if a name has been provided
            if (string.IsNullOrEmpty(patternName))
            {
                Debug.LogError("Please provide a name for the recoil pattern.");
                return;
            }

            // Save the recoil pattern as a ScriptableObject with the given name
            string assetPath = "Assets/Scripts/Items/Weapon/Shoot/ShootSettings/RecoilPattern/" + patternName + ".asset";

            // Check if an asset with the same name already exists
            if (AssetDatabase.LoadAssetAtPath<RecoilPatternData>(assetPath) != null)
            {
                Debug.LogWarning("A recoil pattern with the same name already exists. Overwriting it.");
            }

            RecoilPatternData data = ScriptableObject.CreateInstance<RecoilPatternData>();
            
            NormalizePoints();
            data.pattern = recoilPattern.ToArray();//real points
            data.recoilPatternForVisualization = recoilPatternForVisualization.ToArray();//the points for only visualization
            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Recoil pattern saved.");
        }
    }

}
#endif