using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QSQuestBranchData
{
    [field: SerializeField] public string Text { get; set; }
    [field: SerializeField] public QSQuestSO NextQuestNode { get; set; }
    
}
[Serializable]
public class QSParentData
{
    [field: SerializeField]public string ParentNodeID { get; set; }
    [field: SerializeField] public QSQuestSO PreviousQuestNode { get; set; }
}
