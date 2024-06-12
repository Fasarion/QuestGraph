using System.Collections;
using System.Collections.Generic;
using QS.Enumerations;
using UnityEngine;

public class QSConditionSetterSO : QSQuestSO
{
    public override void Initialize(string nodeName, List<QSQuestBranchData> branches, QSQuestNodeType questNodeType, bool isStartingNode, bool isTestTarget)
    {
        base.Initialize(nodeName, branches, questNodeType, isStartingNode, isTestTarget);
        
        
    }
}
