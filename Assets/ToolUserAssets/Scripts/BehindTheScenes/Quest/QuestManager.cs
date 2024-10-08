using System;
using System.Collections;
using System.Collections.Generic;
using KKD;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestManager", menuName = "Managers/QuestManager" + "")]
public class QuestManager : ScriptableObject
{
    public event Action onAllQuestTasksComplete;
    
    //[NonSerialized]public RectTransform questPanel;
    public List<QuestHandler> questHandlersToComplete;
    public List<QuestHandler> collectibleQuestHandlers;
    public List<QuestHandler> killQuestHandlers;
    public bool allQuestTasksCompleted;
    
    public void CheckCollectible(MonoBehaviour monoBehaviour, QuestCollectible collectible)
    {
        for (int i = 0; i < collectibleQuestHandlers.Count; i++)
        {
            if (collectibleQuestHandlers[i].questAccepted)
            {

                for (int j = 0; j < collectibleQuestHandlers[i].itemTypesToCollect.Count; j++)
                {
                    var collectibleType = collectible.identityBehaviour.GetType();
                    var questHandlerItemType = collectibleQuestHandlers[i].itemTypesToCollect[j].GetType();
                    if (collectible.destroyCollectibleOnPickup && collectibleType == questHandlerItemType)
                    {
                        Destroy(collectible.gameObject);
                    }
                    collectibleQuestHandlers[i].AddItem(monoBehaviour);
                }
                
            }
           
        }
    }
    
    public void CheckKillable(Component monoBehaviour)
    {
        for (int i = 0; i < killQuestHandlers.Count; i++)
        {
            killQuestHandlers[i].AddKill(monoBehaviour);
        }
    }
    
    public void CheckAllNecessaryQuestsCompleted()
    {
        allQuestTasksCompleted = true;
        
        
        for (int i = 0; i < collectibleQuestHandlers.Count; i++)
        {
            if (!collectibleQuestHandlers[i].questTasksComplete)
            {
                allQuestTasksCompleted = false;
            }
        }
        for (int i = 0; i < killQuestHandlers.Count; i++)
        {
            if (!killQuestHandlers[i].questTasksComplete)
            {
                allQuestTasksCompleted = false;
            }
        }
        
        if (allQuestTasksCompleted)
        {
            onAllQuestTasksComplete?.Invoke();
        }
    }
}
