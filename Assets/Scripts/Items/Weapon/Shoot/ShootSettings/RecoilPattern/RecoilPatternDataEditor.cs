#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Item
{
    [CustomEditor(typeof(RecoilPatternData))]
    public class RecoilPatternDataEditor : Editor
    {
        private const float drawAreaSize = 400f; // Size of the drawing area
        private const float pointSize = 3f; // Size of each point in the pattern

        public override void OnInspectorGUI()
        {
            // Call the default inspector GUI to display other fields
            DrawDefaultInspector();

            // Get reference to the RecoilPatternData instance
            RecoilPatternData recoilData = (RecoilPatternData)target;

            // Draw label
            GUILayout.Label("Recoil Pattern Visualization", EditorStyles.boldLabel);

            // Create an area for the pattern to be drawn
            Rect drawArea = GUILayoutUtility.GetRect(drawAreaSize, drawAreaSize);
            EditorGUI.DrawRect(drawArea, Color.black); // Draw the background of the pattern area

            if (recoilData.recoilPatternForVisualization != null && recoilData.recoilPatternForVisualization.Length > 0)
            {
                Handles.color = Color.white;
                Vector2 lastPoint = Vector2.zero;

                // Loop through all the points in the pattern and draw lines
                for (int i = 0; i < recoilData.recoilPatternForVisualization.Length; i++)
                {
                    // Calculate the position of the point within the drawing area
                    Vector2 point = recoilData.recoilPatternForVisualization[i];

                    // Normalize the recoil pattern points to fit within the drawing area
                    Vector2 normalizedPoint = new Vector2(
                        Mathf.Lerp(drawArea.xMin, drawArea.xMax, point.x / drawAreaSize),
                        Mathf.Lerp(drawArea.yMin, drawArea.yMax, point.y / drawAreaSize)
                    );

                    // Draw the point
                    Handles.DrawSolidDisc(normalizedPoint, Vector3.forward, pointSize);

                    // Draw lines between the points to show the pattern path
                    if (i > 0)
                    {
                        Handles.DrawLine(lastPoint, normalizedPoint);
                    }

                    lastPoint = normalizedPoint;
                }
            }
            else
            {
                // If no pattern is available, show a warning
                EditorGUILayout.HelpBox("No recoil pattern available", MessageType.Warning);
            }
        }
    }

}
#endif