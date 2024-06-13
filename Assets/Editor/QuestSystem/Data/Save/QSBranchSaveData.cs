using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS.Data.Save
{
    [Serializable]
    public class QSBranchSaveData
    {
        [field: SerializeField]public string Text { get; set; }
        [field: SerializeField]public string NextNodeID { get; set; }
    }

    [Serializable]
    public class QSParentSaveData
    {
        [field: SerializeField]public string ParentNodeID { get; set; }
    }

}
