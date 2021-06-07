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
    private static GUIContent buttonTest = new GUIContent("Test Shift", "Test");
    private static GUIContent buttonTestLoad = new GUIContent("Test Load", "Test Load");


    public override void OnInspectorGUI()
    {
        p = (AStarGraphSaver)target;
        if (p == null) return;

        serializedObject.Update();


        //Label Color
        var saveColor = EditorStyles.label.normal.textColor;
        EditorStyles.label.normal.textColor = Color.yellow;


        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("LOAD SURROUNDING TERRAIN TILES", GUILayout.MaxWidth(420));
        EditorGUILayout.LabelField("Surrounding terrain tiles must be loaded before scanning A* Recast graph. This will ensure the scan covers the border edges.", GUILayout.MaxWidth(800));
        EditorGUILayout.LabelField("Otherwise the scan will leave a gap at the edge for the player width, because it thinks it won't fit.", GUILayout.MaxWidth(800));
        EditorGUILayout.EndVertical();
        EditorStyles.label.normal.textColor = saveColor;


        // Check if any control changed between here and EndChangeCheck
        EditorGUI.BeginChangeCheck();
        base.DrawDefaultInspector();

        if (GUILayout.Button(buttonSave, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
        {
            p.SaveGraph();
        }

        #region Testing Methods

        if (GUILayout.Button(buttonTest, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
        {
            p.TestShift();
        }

        if (GUILayout.Button(buttonTestLoad, EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
        {
            p.TestLoad();
        }

        #endregion Testing Methods

        // If any control changed, then apply changes
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

}
