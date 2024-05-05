using System.Collections;
using System.Collections.Generic;
using KKD;
using UnityEngine;
using UnityEngine.UI;

public class TestConditionSetterMonoBehaviour : MonoBehaviour
{
    public QuestHandler questHandlerToSet;

    
    // Start is called before the first frame update
    public void OnConditionSetButtonPressed()
    {
        questHandlerToSet.QuestTasksCompleted();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
