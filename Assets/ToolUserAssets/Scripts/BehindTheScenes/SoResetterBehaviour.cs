using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKD;
using Unity.VisualScripting;
using UnityEngine;

public class SoResetterBehaviour : MonoBehaviour
{
    
    //public List<QuestHandler> questHandlers;

    public bool resetQuest;

    public List<QuestRuntimeManager> questRuntimeManagers;
    private void Start()
    {

        var questRuntimeManagerArray = FindObjectsOfType<QuestRuntimeManager>(true);
        questRuntimeManagers = questRuntimeManagerArray.ToList();
        foreach (QuestRuntimeManager questRuntimeManager in questRuntimeManagers)
        {
            //Ugly, but should work.
            if (questRuntimeManager.questData.questHandlerSOs != null && questRuntimeManager.questData.questHandlerSOs.Count != 0)
            {
                foreach (var questHandlerSO in questRuntimeManager.questData.questHandlerSOs)
                {
                    if (questRuntimeManager.resetToActive)
                    {
                        questHandlerSO.QuestHandler.ResetQuest(true);
                    }
                    else if (questRuntimeManager.resetToInactive)
                    {
                        questHandlerSO.QuestHandler.ResetQuest(false);
                    }
                   
                }
            }
        }
        /*if (resetQuest)
        {
            foreach (var questHandler in questHandlers)
            {
                questHandler.ResetKills();
                questHandler.ResetItemCollect();
                questHandler.ResetFetch();
                questHandler.ResetQuestState();
                questHandler.questAccepted = false;
                questHandler.questActive = false;
                questHandler.questComplete = false;
            }
        }*/
       
        

    }



}
