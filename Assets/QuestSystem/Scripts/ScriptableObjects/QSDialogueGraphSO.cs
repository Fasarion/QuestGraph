using System.Collections;
using System.Collections.Generic;
using DS.ScriptableObjects;
using KKD;
using QS.Enumerations;
using UnityEngine;

public class QSDialogueGraphSO : QSQuestSO
{
    [field: SerializeField] public DSDialogueContainerSO DialogueContainerSO { get; set; }

    private string branchName;
    private float testFloat;
    private QuestHandler testHandler;
   

    public void Initialize(string nodeName,List<QSQuestBranchData> branches, DSDialogueContainerSO dialogueContainerSo, QSQuestNodeType questNodeType, bool isStartingNode, bool isTestTarget, QSParentData parentData)
    {
        base.Initialize(nodeName,branches, questNodeType,isStartingNode,isTestTarget, parentData );
        DialogueContainerSO = dialogueContainerSo;
        

    }
}
