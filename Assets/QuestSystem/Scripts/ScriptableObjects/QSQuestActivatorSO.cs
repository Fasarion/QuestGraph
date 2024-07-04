using System.Collections;
using System.Collections.Generic;
using KKD;
using QS.Enumerations;
using UnityEngine;

public class QSQuestActivatorSO : QSQuestSO
{
    [field: SerializeField] public QuestHandler QuestHandler { get; set; }
    [field: SerializeField] public bool AcceptOnActivate { get; set; }

    public void Initialize(string nodeName,List<QSQuestBranchData> branches, QuestHandler questHandler, QSQuestNodeType questNodeType, bool isStartingNode, bool isTestTarget, QSParentData parentData, bool acceptOnActivate)
    {
        base.Initialize(nodeName,branches, questNodeType,isStartingNode,isTestTarget, parentData);
        QuestHandler = questHandler;
        AcceptOnActivate = acceptOnActivate;

    }
}
