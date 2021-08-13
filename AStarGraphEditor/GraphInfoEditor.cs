using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tools.AStarGraphEditor
{

    [CustomEditor(typeof(GraphInfo))]
    public class GraphInfoEditor : Editor
    {

        GraphInfo p;

        private static GUIContent buttonMakeGraph = new GUIContent("MakeGraph", "Make Graph");

        public override void OnInspectorGUI()
        {
            p = (GraphInfo)target;
            if (p == null) return;

            serializedObject.Update();


            //Label Color
            var saveColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = Color.yellow;


            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Update recast graph with mesh", GUILayout.MaxWidth(420));
            EditorGUILayout.EndVertical();
            EditorStyles.label.normal.textColor = saveColor;


            // Check if any control changed between here and EndChangeCheck
            EditorGUI.BeginChangeCheck();
            base.DrawDefaultInspector();

            if (GUILayout.Button(buttonMakeGraph, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
            {
                p.MakeGraph();
            }


            // If any control changed, then apply changes
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}