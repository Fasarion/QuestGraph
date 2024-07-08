using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(QuestRuntimeManager))]
public class QuestRuntimeManagerEditor : Editor
{
    private SerializedProperty questData;
    private SerializedProperty currentNode;
    private SerializedProperty conditionMet;
    
    private SerializedProperty autoTest;
    private SerializedProperty autoPlayTrigger;
    //private SerializedProperty resetQuestToActive;
    //private SerializedProperty resetQuestToInactive;
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
      
        questData = serializedObject.FindProperty("questData");
        currentNode = serializedObject.FindProperty("currentNode");
        conditionMet = serializedObject.FindProperty("conditionMet");
        autoTest = serializedObject.FindProperty("autoTest");
        autoPlayTrigger = serializedObject.FindProperty("autoPlayTrigger");


        //resetQuestToActive = serializedObject.FindProperty("resetToActive");
        //resetQuestToInactive = serializedObject.FindProperty("resetToInactive");

        EditorGUILayout.PropertyField(questData);
        EditorGUILayout.PropertyField(currentNode);
        EditorGUILayout.PropertyField(conditionMet);
        EditorGUILayout.PropertyField(autoTest);
        EditorGUILayout.PropertyField(autoPlayTrigger);
        //EditorGUILayout.PropertyField(resetQuestToActive);
        //EditorGUILayout.PropertyField(resetQuestToInactive);
        
        
        QuestRuntimeManager runtimeManager = (QuestRuntimeManager)target;

        // Begin checking for changes in the inspector
        EditorGUI.BeginChangeCheck();

        // Display the toggles and handle their logic
        bool newResetToActive = EditorGUILayout.Toggle("Reset Quest To Active", runtimeManager.resetToActive);
        if (newResetToActive != runtimeManager.resetToActive)
        {
            runtimeManager.resetToActive = newResetToActive;
            if (newResetToActive)
            {
                runtimeManager.resetToInactive = false;
            }
        }

        bool newResetToInactive = EditorGUILayout.Toggle("Reset Quest To Inactive", runtimeManager.resetToInactive);
        if (newResetToInactive != runtimeManager.resetToInactive)
        {
            runtimeManager.resetToInactive = newResetToInactive;
            if (newResetToInactive)
            {
                runtimeManager.resetToActive = false;
            }
        }

        // Apply changes if any were made
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(runtimeManager);
        }


    }
}
