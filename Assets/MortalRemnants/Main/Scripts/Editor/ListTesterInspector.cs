using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(ListTester))]
public class ListTesterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorList.Show(serializedObject.FindProperty("integers"));
        EditorList.Show(serializedObject.FindProperty("vectors"));
        EditorList.Show(serializedObject.FindProperty("colorPoints"));
        EditorList.Show(serializedObject.FindProperty("objects"));
        EditorList.Show(serializedObject.FindProperty("questStates"));
        serializedObject.ApplyModifiedProperties();
    }
}
