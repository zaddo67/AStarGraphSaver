using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tools.AStarGraphEditor
{

    [CustomEditor(typeof(GraphToMesh))]

    public class GraphToMeshEditor : Editor
    {

        GraphToMesh p;

        private static GUIContent buttonMakeMesh = new GUIContent("MakeMesh", "Make Mesh");
        private static GUIContent buttonClearMesh = new GUIContent("ClearMesh", "Clear Mesh");


        public override void OnInspectorGUI()
        {
            p = (GraphToMesh)target;
            if (p == null) return;

            serializedObject.Update();


            //Label Color
            var saveColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = Color.yellow;


            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Convert AStar Recast graph into a mesh.", GUILayout.MaxWidth(800));
            EditorGUILayout.LabelField("Edit mesh and use child component to", GUILayout.MaxWidth(800));
            EditorGUILayout.LabelField("update Recast graph with changes", GUILayout.MaxWidth(800));
            EditorGUILayout.EndVertical();
            EditorStyles.label.normal.textColor = saveColor;


            // Check if any control changed between here and EndChangeCheck
            EditorGUI.BeginChangeCheck();
            base.DrawDefaultInspector();

            if (GUILayout.Button(buttonMakeMesh, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
            {
                p.GenerateMesh();
            }


            if (GUILayout.Button(buttonClearMesh, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
            {
                p.ClearMesh();
            }

            // If any control changed, then apply changes
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

    }
}