using System;
using System.Collections.Generic;
using System.Linq;
using DS.Utilities;
using KKD;
using QS.Data.Save;
using QS.Enumerations;
using QS.Utilities;
using QS.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace QS.Elements
{
    public class QSNode : Node
    {
        public string ID { get; set; }
        public string NodeName { get; set; }
        protected QSGraphView graphView;
        private Color defaultBackgroundColor;

        public QSQuestNodeType QuestNodeType;
        public List<QSBranchSaveData> branches;

        protected List<PropertyField> propertyFields;
        private VisualElement textBoxContainer;
        protected VisualElement customDataContainer;
        private Toggle testFromThisToggle;
        public bool testTarget;
        public string oldNameText;
        public QSParentSaveData parentSaveData;
        
        public virtual void Initialize(string nodeName, QSGraphView qsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            nodeName = CheckForDuplicateNames(nodeName, qsGraphView);
            NodeName = nodeName;
            branches = new List<QSBranchSaveData>();
            parentSaveData = new QSParentSaveData()
            {
                ParentNodeID = null
            };
            graphView = qsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
            
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("qs-node__main-container");
            mainContainer.AddToClassList("qs-node__extension-container");
            propertyFields = new List<PropertyField>();
            qsGraphView.currentTestNodeUpdated += OnCurrentTestNodeUpdated;



        }

        
        //Should be ignored by the testNode that was just tagged as the current one
        private void OnCurrentTestNodeUpdated(QSNode testNode)
        {
            if (testFromThisToggle != null)
            {
                if (testNode != this)
                {
                    if (testFromThisToggle.value)
                    {
                        testFromThisToggle.SetValueWithoutNotify(false);
                        testTarget = false;
                    }
                }
                
            }
           
        }

        public virtual void OnDestroy(QSGraphView qsGraphView)
        {
            qsGraphView.currentTestNodeUpdated -= OnCurrentTestNodeUpdated;
        }
        
        
        
        
        


        public virtual void Draw()
        {
            
            testFromThisToggle = QSElementUtility.CreateToggle("Test from this Node", callback =>
            {
                
                testFromThisToggle.value = callback.newValue;
                testTarget = callback.newValue;
                graphView.currentTestNode = this;
                graphView.SendCurrentTestNodeUpdatedEvent(this);
               
                //ToggleAutoPlay(callback.newValue);
            });
            testFromThisToggle.SetValueWithoutNotify(testTarget);
            
           
            //That's right, we wanted to figure out if there's a way to get a callback on deselecting the text field
            TextField dialogueNameTextField = QSElementUtility.CreateTextField(NodeName, null,callback =>
            {
                TextField target = (TextField)callback.target;
                
                
                
                
            });
            oldNameText = NodeName;
            
            
            dialogueNameTextField.RegisterCallback<FocusOutEvent>(callback =>
            {
                
                TextField target = (TextField)callback.target;
                if (target.value == NodeName)
                {
                    
                }
                else
                {
                    
                  
                    var oldValue = NodeName;
                    graphView.uniqueNodeNames.Remove(oldValue);
                    var newValue = CheckForDuplicateNames(target.value, graphView);
               
                    target.value = newValue;
                    NodeName = newValue;
                    
                    /*if (string.IsNullOrEmpty(target.value))
                    {
                        if (!string.IsNullOrEmpty(NodeName))
                        {
                            ++graphView.NameErrorsAmount;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(NodeName))
                        {
                            --graphView.NameErrorsAmount;
                        }
                    }*/
                    
                }
                Debug.Log("TextField no longer in focus");
               
                
            });

            dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__filename-textfield",
                "ds-node__textfield__hidden"
            );


            titleContainer.Insert(0,dialogueNameTextField);
            
            Port inputPort = this.CreatePort("Quest Connection", Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi);
            inputPort.portName = "Quest Connection";
            
            //Holds the ID for the node the comes before the current one.

            if (parentSaveData != null)
            {
                if (string.IsNullOrEmpty(parentSaveData.ParentNodeID))
                {
                    parentSaveData = new QSParentSaveData()
                    {
                        ParentNodeID = ""
                    };
                }
                inputPort.userData = parentSaveData;
            }
            else
            {
                parentSaveData = new QSParentSaveData()
                {
                    ParentNodeID = ""
                };
                inputPort.userData = parentSaveData;
            }
           
            
          
            
            inputContainer.Add(inputPort);

            customDataContainer = new VisualElement();
            
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            
            VisualElement selection = this.Q("selection-border", (string)null);


            if (selection != null)
            {
                selection.AddClasses("qs-node__selection");
            }
            
            extensionContainer.Add(customDataContainer);
            extensionContainer.Add(testFromThisToggle);
           
            
            RefreshExpandedState();

        }

        #region Overrided Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
        }

        #endregion
        
        #region Utility Methods
        
        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        
        
        
        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }
        
        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }
        
        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }
                graphView.DeleteElements(port.connections);
            }
        }
        
        public bool IsStartingNode()
        {
            if(inputContainer.Children().Count() != 0)
            {
                Port inputPort = (Port)inputContainer.Children().First();
                return !inputPort.connected;
            }
            
            return false;

        }

        public bool IsTestTargetNode()
        {
            return testTarget;
        }
        
        public void SplitStringAtIndex(int index,string inputString, out string string1, out string string2)
        {
            if (inputString.Length >= index)
            {
                string1 = inputString.Remove(index);
                string2 = inputString.Substring(index);
            }
            else if(inputString.Length == 1)
            {
                string1 = inputString.Remove(index);
                string2 = inputString.Substring(index);
            }
            else
            {
                string2 = string.Empty;
                string1 = string.Empty;
            }
        }
        
        public string CheckForDuplicateNames(string nodeName, QSGraphView graphview)
        {
            

            int number = 0;
            var iterationCutoff = 0;
            while(graphview.uniqueNodeNames.Contains(nodeName) && iterationCutoff < 300)
            {
                
                //Remember to remove this once you have verified that it works.
                iterationCutoff++;
                SplitStringAtIndex(nodeName.Length-2, nodeName, out string nodeNameStart, out string nodeNameEnd);
                var nodeNameEndWithoutEndNumbers = new string(nodeNameEnd.Where(char.IsLetter).ToArray());
                number += 1;
                nodeName = nodeNameStart + nodeNameEndWithoutEndNumbers + number;
                
                
                
            }

            graphview.uniqueNodeNames.Add(nodeName);
            
            return nodeName;
        }
        
        #endregion
    }
}

