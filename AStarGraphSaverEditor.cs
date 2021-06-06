using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pathfinding;
using Pathfinding.Util;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[CustomEditor(typeof(AStarGraphSaver))]
public class AStarGraphSaverEditor : Editor
{

    AStarGraphSaver p;

    private static GUIContent buttonSave = new GUIContent("Save", "Save Graph");


    public override void OnInspectorGUI()
    {
        p = (AStarGraphSaver)target;
        if (p == null) return;

        serializedObject.Update();

        // Check if any control changed between here and EndChangeCheck
        EditorGUI.BeginChangeCheck();
        base.DrawDefaultInspector();

        if (GUILayout.Button(buttonSave, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
        {
            p.SaveGraph();
        }

        // If any control changed, then apply changes
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

}
