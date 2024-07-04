using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DS.ScriptableObjects;
using KKD;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//The functions in this script run on the assumption that if there is no node to transition to, it will simply cease to iterate over itself
//And wait for some kind of external change that affects the state.
public class QuestRuntimeManager : MonoBehaviour
{
    private DialogueManager dialogueManager;
    public QSQuestDataContainerSO questData;

    public QSQuestSO currentNode;
    
    private QuestHandler questHandler;

    public Action<QuestHandler> sendQuestHandlerSetEvent;

    public bool conditionMet = false;
    
    public bool autoTest;

    private QSQuestSO testTarget;
    private QSQuestSO currentTestPathNode;
    private HashSet<QSQuestSO> currentTestPath;
    private Dictionary<int, QSQuestSO> indexByCurrentPathNodes;
    //Do not use this to access the dictionary.
    private int currentPathIndexFromTestNode;
    //Use this to access the dictionary. Remember that we have to start from the end of the dictionary.
    private int testPathNodeIndex;
    
    private List<GameObject> gameObjectsInSceneAtStart;

    public DialogueTrigger autoPlayTrigger;

    
    // Start is called before the first frame update
    void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
       
        if (questData != null)
        {
            if (questData.questHandlerSOs != null)
            {
                if (questData.questHandlerSOs.Count > 1)
                {
                    Debug.LogError("Error! Multiple quest handlers in the graph, quest will not function as expected.");
                }
                else
                {
                    currentNode = questData.startingNode;
                    var questNode = questData.questHandlerSOs[0];
                    questHandler = questNode.QuestHandler;
                    if (autoTest)
                    {
                        testTarget = questData.testTargetNode;
                        if (testTarget != null)
                        {
                            //currentTestPath = new HashSet<QSQuestSO>();
                            indexByCurrentPathNodes = new Dictionary<int, QSQuestSO>();
                            currentPathIndexFromTestNode = 0;
                            AddTestNodesRecursively(testTarget, currentPathIndexFromTestNode);
                            var listIndexInPath = indexByCurrentPathNodes.Count-1;
                            testPathNodeIndex = listIndexInPath;
                        }
                       
                        
                        
                    }
                }
            }
        }
    }
    
    private void Start()
    {
        //This is probably going to be VERY slow when we have 1000s of enemies in the scene. It would at least increase load times. Might have to 
        //resort to some kind of assignment logic for scene objects after all. Could have a base class for all game objects that should be saved
        //in the scene and add them into a list of objects to activate/deactivate. Then we could leave spawned enemies created at runtime out 
        //of that list. Anyway, it is out of scope for the school assignment.
        //
        //Actually since we're using entities and those are possibly spawned later it is possible it won't be an issue. Also we could maybe do the 
        //initialization a frame later in order to create a buffer to only get the scene objects before all objects are initialized. Another way
        //would be to get the children of some game object and not include the spawned enemies under that object, that is also likely to be more
        //performant and an easier solution as well.
        
        gameObjectsInSceneAtStart = GetAllObjectsOnlyInScene();
        if (autoTest && testTarget != null)
        {
            questHandler.questActive = true;
            CheckAutoTransitionCondition();
            //CheckNodeTransitionCondition();
        }
        else
        {
            if (questHandler.questActive)
            {
                CheckNodeTransitionCondition();
            }
        }
        
       
        
        //currentNode = questData.
        
    }


    

    public void AddTestNodesRecursively(QSQuestSO nodePartOfTestPath, int currentDictionaryIndex)
    {
        currentTestPathNode = nodePartOfTestPath;

        //Left for testing purposes
        /*if(!currentTestPath.Contains(currentTestPathNode))
       {
           currentTestPath.Add(currentTestPathNode);
           if (currentTestPathNode.ParentData.PreviousQuestNode != null)
           {
               AddTestNodesRecursively(currentTestPathNode.ParentData.PreviousQuestNode, currentDictionaryIndex);
           }
       }*/
        
        if (!indexByCurrentPathNodes.ContainsValue(currentTestPathNode))
        {
            if (indexByCurrentPathNodes.TryAdd(currentDictionaryIndex, currentTestPathNode))
            {
                Debug.Log("Node " + currentTestPathNode + " added at index: " + currentDictionaryIndex);
            }
            else
            {
                Debug.LogError("It was not possible to add the path node to that index. Is the index already in use?");
            }
            if (currentTestPathNode.ParentData.PreviousQuestNode != null)
            {
                currentDictionaryIndex++;
                AddTestNodesRecursively(currentTestPathNode.ParentData.PreviousQuestNode, currentDictionaryIndex);
            }
        }
        else
        {
            Debug.LogError("An attempt was made to add the same node twice to the Test Node Path. This implies a loop in the graph which AutoTesting currently does not support." +
                           "Remove the looping quest nodes and try again.");
        }
       
    }

    public void AutoTestTriggerDialogue(DSDialogueContainerSO dialogueContainerSo)
    {
        autoPlayTrigger.SetDialogueAtRuntimeAndTrigger(dialogueContainerSo);
    }

    private void OnEnable()
    {
        dialogueManager.reachedLastDialogueSO += OnReachedLastDialogueSO;
        questHandler.onQuestTasksCompleted += OnQuestConditionMet;
    }

    private void OnDisable()
    {
        dialogueManager.reachedLastDialogueSO -= OnReachedLastDialogueSO;
        questHandler.onQuestTasksCompleted -= OnQuestConditionMet;
    }

   
    public void CheckAutoTransitionCondition()
    {
        //Time check to break loop if it becomes infinite.
        while (testPathNodeIndex >= 0)
        {
            indexByCurrentPathNodes.TryGetValue(testPathNodeIndex, out currentNode);
            testPathNodeIndex--;
                
                if(currentNode.GetType() == typeof(QSActivatorSO))
                {
                    var activatorNode = (QSActivatorSO)currentNode;
            
                    ActivateAndDeactivateGameObjects(activatorNode);
                    //activatorNode.GameObjectNames
                }
                else if (currentNode.GetType() == typeof(QSQuestAcceptedSO))
                {
                    questHandler.questAccepted = true;
                }
                else if (currentNode.GetType() == typeof(QSQuestActivatorSO))
                {
                    var questActivatorNode = (QSQuestActivatorSO)currentNode;
                    questActivatorNode.QuestHandler.QuestActivated();
                    if (questActivatorNode.AcceptOnActivate)
                    {
                        questActivatorNode.QuestHandler.questAccepted = true;
                    }
                }
                else if (currentNode.GetType() == typeof(QSQuestHandlerSO))
                {
                    //Should not be possible.
                }
                else if (currentNode.GetType() == typeof(QSConditionSetterSO))
                {
                    conditionMet = true;
                    //This feels redundant or in the wrong place.
                    if (questHandler.questType == QuestType.TalkToQuest)
                    {
                        questHandler.QuestTasksCompleted();
                    }
                }
                else if (currentNode.GetType() == typeof(QSConditionSO))
                {
                    conditionMet = true;
                }
                else if (currentNode.GetType() == typeof(QSDialogueGraphSO))
                {
                    var dialogueNode = (QSDialogueGraphSO)currentNode;
                    var dialogueContainer = dialogueNode.DialogueContainerSO;
                    AutoTestTriggerDialogue(dialogueContainer);
                    //Avoids a loop while we're waiting for the system to handle the dialogue
                    break;
                }
        }

        //We've reached the final node
        if (testPathNodeIndex < 0)
        {
            if (currentNode == testTarget)
            {
                autoTest = false;
                Debug.Log("Reached AutoTest Target: " + currentNode.name);
                CheckNodeTransitionCondition();
            }
            
        }
       
    }
    public void RunAutoTest()
    {
        
    }

    public void OnQuestActivated(QuestHandler handler)
    {
        if (handler == questHandler)
        {
            CheckNodeTransitionCondition();
        }
    }

    public void AcceptQuest()
    {
        if (questHandler.questAccepted == false)
        {
            questHandler.questAccepted = true;
        }
    }

    List<GameObject> GetAllObjectsOnlyInScene()
    {
        List<GameObject> objectsInScene = new List<GameObject>();

        var sceneName = SceneManager.GetActiveScene().name;
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (!EditorUtility.IsPersistent(gameObject.transform.root.gameObject) && !(gameObject.hideFlags == HideFlags.NotEditable || gameObject.hideFlags == HideFlags.HideAndDontSave))
                if (gameObject.scene.name == sceneName)
                {
                    objectsInScene.Add(gameObject);
                }
                
        }

        return objectsInScene;
    }
    //Maybe I should make runtime classes for each node since they have logic attached to them?
    public void CheckNodeTransitionCondition()
    {
        if(currentNode.GetType() == typeof(QSActivatorSO))
        {
            var activatorNode = (QSActivatorSO)currentNode;
            
            ActivateAndDeactivateGameObjects(activatorNode);
            //activatorNode.GameObjectNames
        }
        else if (currentNode.GetType() == typeof(QSQuestAcceptedSO))
        {
            questHandler.questAccepted = true;
            GoToNextNode(currentNode.Branches[0]);
        }
        else if (currentNode.GetType() == typeof(QSQuestActivatorSO))
        {
            var questActivatorNode = (QSQuestActivatorSO)currentNode;
            questActivatorNode.QuestHandler.QuestActivated();
            if (questActivatorNode.AcceptOnActivate)
            {
                questActivatorNode.QuestHandler.questAccepted = true;
            }
            if (currentNode.Branches.Count > 0)
            {
                GoToNextNode(currentNode.Branches[0]);
            }
            
        }
        else if (currentNode.GetType() == typeof(QSQuestHandlerSO))
        {
            //Should not be possible.
        }
        else if (currentNode.GetType() == typeof(QSConditionSetterSO))
        {
            conditionMet = true;
            GoToNextNode(currentNode.Branches[0]);
            //This feels redundant or in the wrong place.
            if (questHandler.questType == QuestType.TalkToQuest)
            {
                questHandler.QuestTasksCompleted();
            }
            
            
        }
        else if (currentNode.GetType() == typeof(QSConditionSO))
        {
            if (autoTest && testTarget != null)
            {
                conditionMet = true;
            }
            if (conditionMet)
            {
                GoToNextNode(currentNode.Branches[0]);
            }
            
        }
        else if (currentNode.GetType() == typeof(QSDialogueGraphSO))
        {
            if (autoTest && testTarget != null)
            {
                var dialogueNode = (QSDialogueGraphSO)currentNode;
                var dialogueContainer = dialogueNode.DialogueContainerSO;
                AutoTestTriggerDialogue(dialogueContainer);
                
                //Activate the current dialogue
            }
        }
        else
        {
            //This would mean there is no node to transition to so the quest is just going to stop here instead.
            //Debug.LogError("Current node is null, no transition possible.");
        }

    }

    public void ActivateAndDeactivateGameObjects(QSActivatorSO activatorNode)
    {

        //gameObjectsInSceneAtStart.RemoveAll(e => e == null);
        for (int i = 0; i < gameObjectsInSceneAtStart.Count; i++)
        {
            var currentObject = gameObjectsInSceneAtStart[i].gameObject;
            foreach (var objectName in activatorNode.GameObjectsToActivateNames)
            {
                if (currentObject != null)
                {
                    if (objectName == currentObject.name)
                    {
                        currentObject.SetActive(true);
                    }
                }
                
            }
            foreach (var objectName in activatorNode.GameObjectsToDeactivateNames)
            {
                if (currentObject != null)
                {
                    if (objectName == currentObject.name)
                    {
                        currentObject.SetActive(false);
                    }
                }
               
            }
        };
        if (!autoTest)
        {
            GoToNextNode(currentNode.Branches[0]);
        }
        
    }
    

    public void OnReachedLastDialogueSO(DSDialogueSO dialogueSo)
    {
        SelectDialogueGraphBranch(dialogueSo);
    }

    public void OnQuestConditionMet()
    {
        //Maybe we can do a null check here, and if the current node is empty we just move on. 
        //Debug.Log("Condition fulfilled!");
        conditionMet = true;
        if (currentNode != null)
        {
            if (currentNode.GetType() == typeof(QSConditionSO))
            {
                GoToNextNode(currentNode.Branches[0]);
            }
        }
        
    }
    //We have to set the chosen path based on what dialogue is played out.
    public void SelectDialogueGraphBranch(DSDialogueSO dialogueSo)
    {
        if (autoTest && testTarget !=null)
        {
            CheckAutoTransitionCondition();
        }
        else
        {
            if (currentNode is QSDialogueGraphSO dialogueGraphSo)
            {
                var found = dialogueGraphSo.DialogueContainerSO.exitDialogues.Find(e => e == dialogueSo);
                QSQuestBranchData chosenBranch = null;
                if (found != null)
                {
                    foreach (QSQuestBranchData qsQuestBranchData in dialogueGraphSo.Branches)
                    {
                        if (qsQuestBranchData.Text == found.name)
                        {
                            chosenBranch = qsQuestBranchData;
                        }
                    }

                    if (chosenBranch != null)
                    {
                   
                        GoToNextNode(chosenBranch);
                    
                    }
                    else
                    {
                        Debug.LogWarning("No branch was found to transition to, is this intended?");
                    }
                }
            }
        }
        
    }
    
    public void GoToNextNode(QSQuestBranchData questBranch)
    { 
        if (questBranch != null)
        {
            currentNode = questBranch.NextQuestNode;
            if (currentNode != null)
            {
                if (currentNode == testTarget)
                {
                    autoTest = false;
                    Debug.Log("Reached AutoTest Target: " + currentNode.name);
                }
                CheckNodeTransitionCondition();
            }
            else
            {
                if (questHandler.questAccepted)
                {
                    questHandler.QuestCompleted(questHandler);
                }
                else
                {
                    //Why would I set the currentNode to be the startingNode if the quest is not accepted and we call go to nextNode I wonder?
                    //Maybe I need to check that it is also a dialogueNode since this will obviously causes a loop.
                    Debug.LogWarning("Note that if the quest was not set as accepted, this will cause a stack overflow since it will keep calling nodes.");
                    //currentNode = questData.startingNode;
                    //If we have reached the end here we will simply say the quest is completed automatically for now.
                    //it might cause problems later, but it is what it is right now.
                    
                    questHandler.questAccepted = true;
                    questHandler.QuestCompleted(questHandler);
                    //CheckNodeTransitionCondition();
                }
                
                
            }
            
        }
        else
        {
            if (questHandler.questAccepted)
            {
                questHandler.QuestCompleted(questHandler);
            }
            else
            {
                Debug.LogError("A quest was traversed without being accepted by the player!");
                //I am seriously not sure why I am doing this.
                
                //currentNode = questData.startingNode;
                //CheckNodeTransitionCondition();
            }
            
        }
    }
    
    
    public void GoToNextNodeAutomatically(QSQuestBranchData questBranch)
    { 
        if (questBranch != null)
        {
            //You have to make this into a dictionary my dude!
            //currentNode = currentTestPath[testPathNodeIndex];
            if (currentNode != null)
            {
                if (currentNode == testTarget)
                {
                    autoTest = false;
                    Debug.Log("Reached AutoTest Target: " + currentNode.name);
                }
                CheckNodeTransitionCondition();
            }
            else
            {
                if (questHandler.questAccepted)
                {
                    questHandler.QuestCompleted(questHandler);
                }
                else
                {
                    //Why would I set the currentNode to be the startingNode if the quest is not accepted and we call go to nextNode I wonder?
                    //Maybe I need to check that it is also a dialogueNode since this will obviously causes a loop.
                    Debug.LogWarning("Note that if the quest was not set as accepted, this will cause a stack overflow since it will keep calling nodes.");
                    //currentNode = questData.startingNode;
                    //If we have reached the end here we will simply say the quest is completed automatically for now.
                    //it might cause problems later, but it is what it is right now.
                    
                    questHandler.questAccepted = true;
                    questHandler.QuestCompleted(questHandler);
                    //CheckNodeTransitionCondition();
                }
                
                
            }
            
        }
        else
        {
            if (questHandler.questAccepted)
            {
                questHandler.QuestCompleted(questHandler);
            }
            else
            {
                Debug.LogError("A quest was traversed without being accepted by the player!");
                //I am seriously not sure why I am doing this.
                
                //currentNode = questData.startingNode;
                //CheckNodeTransitionCondition();
            }
            
        }
    }
}
