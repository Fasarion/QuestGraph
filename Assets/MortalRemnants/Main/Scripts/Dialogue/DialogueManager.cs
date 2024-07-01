using System;
using System.Collections;
using System.Collections.Generic;
using DS.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


    public delegate void SetDialogue();
public class DialogueManager : MonoBehaviour
    {
        //Used for telling the player to stop in its place when within a dialogue
        public event SetDialogue dialogueOpened;
        //Used for making the player move again once the dialogue is concluded.
        public event SetDialogue dialogueClosed;
        public event SetDialogue dialogueOptionsAdded;
        public event Action<DSDialogueSO> reachedLastDialogueSO;

        [Header("References")]
        public TMP_Text titleText;
        private Queue<string> _mNames;
        public TMP_Text dialogueText;
        public GameObject scrollWindowBackground;
        public GameObject scrollContent;
        public Button buttonPrefab;
        //public Button button2;
        public TMP_Text buttonText;
        //public TMP_Text buttonText2;
        private Queue<string> _mSentences;
        //private AudioSource _audioSource;
        private Queue<AudioClip> _mAudioClips;
        private Queue<Color> _mColors;

        //This should be for the autoplaying dialogues I think
        [Header("Control")]
        [NonSerialized]public bool useAutomaticDialogueSkip = false;
        [SerializeField]private float timeBeforeAutomaticSkip;
        
        [Header("Debug")]
        public bool branchChoiceDialogueBoxOpen;
        //public List<GameObject> playerBlockers;
        //public bool isUsingVoiceLine = false;

       
        public Queue<Dialogue> dialogues;

        public float dialogueSkipTimer;
        
        public Animator animator;
        public int letterDelay = 1;
        public bool dialogueOpen = false;
        public bool dialogueAdded = false;
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        private DialogueContainer dialogueContainer;
        private DialogueContainer chosenDialogue;
        private DialogueContainer lastChosen;
        private ContainerType currentContainerType;

        private SerializableDictionary<string, DialogueContainer> dialogueOptions;
        private List<string> dialogueOptionNames;

        private IEnumerator typeSentence;

        public bool firstTimeDialogueOpened;

        private float standardFontSize;

        public QuestInternalGraphBehaviour currentInternalGraph;

        public bool autoPlayDialogue;
        
        private void Awake()
        {
            dialogues = new Queue<Dialogue>();
            dialogueOptionNames = new List<string>();
            standardFontSize = dialogueText.fontSize;
        }

        private void Start()
        {
            _mSentences = new Queue<string>();
            _mNames = new Queue<string>();
            //_audioSource = GetComponent<AudioSource>();
            _mAudioClips = new Queue<AudioClip>();
            _mColors = new Queue<Color>();
            
            StartCoroutine(DisplayDialogue());
        }

        public void StartDialogue (Dialogue dialogue)
        {
            dialogueOpen = true;
            //Check that something is subbed to this one.
            dialogueOpened?.Invoke();
            animator.SetBool(IsOpen, true);

            _mSentences.Clear();
            _mNames.Clear();
            _mAudioClips.Clear();
            _mColors.Clear();

            
            foreach (var sentence in dialogue.sentences)
            {
                _mSentences.Enqueue(sentence.text);
                _mNames.Enqueue(sentence.name);
                _mAudioClips.Enqueue(sentence.voiceLine);
                _mColors.Enqueue(sentence.textColor);
            }
            firstTimeDialogueOpened = false;
            DisplayNextSentence();
        }

        public void StartAutoplayDialogue(Dialogue dialogue)
        {
            dialogueOpen = true;
            dialogueOpened?.Invoke();
            _mSentences.Clear();
            _mNames.Clear();
            _mAudioClips.Clear();
            _mColors.Clear();
            
            //Probably more efficient to just enqueue the text and the names but we'll leave it like this for now
            foreach (var sentence in dialogue.sentences)
            {
                _mSentences.Enqueue(sentence.text);
                _mNames.Enqueue(sentence.name);
                _mAudioClips.Enqueue(sentence.voiceLine);
                _mColors.Enqueue(sentence.textColor);
            }
            
            firstTimeDialogueOpened = false;
            AutoplayNextSentence();
        }

        public void AutoplayNextSentence()
        {
            if (firstTimeDialogueOpened == false)
            {
                 //We check if there are sentences, if not we run the End Dialogue function
                if (_mSentences.Count == 0) 
                { 
                    EndDialogue(); 
                    return; 
                }

                string sentence;
                string name;
                AudioClip voiceLine;
                Color textColor;
                //If there are sentences and the container type is a branching dialogue
                if (_mSentences.Count == 1 && dialogueContainer.containerType == ContainerType.BranchingDialogue)
                {
                    
                    //Get the name of the first branch in the dialogueContainer. We always autoplay the first branch for now.
                    dialogueOptions.TryGetValue(dialogueContainer.branches[0].name, out chosenDialogue);
                    Debug.Log("Dialogue choice autoplayed");
                    sentence = _mSentences.Dequeue();
                    name = _mNames.Dequeue();
                    voiceLine = _mAudioClips.Dequeue(); 
                     textColor = _mColors.Dequeue(); 
                    AutoplayNextSentence();
                    AutoplayNextSentence();

                }
                else
                {
                    branchChoiceDialogueBoxOpen = false;
                }
                
                //We then dequeue the sentences, names, voice lines, and the color of our text for the given dialogue so that we can move on with the next set.
                Debug.Log("Sentence autodequeued");
                if (_mSentences.Count > 0)
                {
                    sentence = _mSentences.Dequeue();
                }

                if (_mNames.Count > 0)
                {
                    name = _mNames.Dequeue();
                }

                if (_mAudioClips.Count > 0)
                {
                    voiceLine = _mAudioClips.Dequeue(); 
                }

                if (_mColors.Count > 0)
                {
                    textColor = _mColors.Dequeue(); 
                }
                
                
                 
                 
                 
            }
        }

        
        //Updates sentences in sequence in the order they are shown.
        //
        public void DisplayNextSentence()
        { 
            //If this isn't the first time the dialogue is opened
            if (firstTimeDialogueOpened == false)
            {
                //We check if there are sentences, if not we run the End Dialogue function
                if (_mSentences.Count == 0)
                {
                    EndDialogue();
                    return;
                }
            
            
                //If there are sentences and the container type is a branching dialogue, we open the choice box and we show the dialogue options.
                if (_mSentences.Count == 1 && dialogueContainer.containerType == ContainerType.BranchingDialogue)
                {
                    
                    branchChoiceDialogueBoxOpen = true;
                    Debug.Log("DialogueChoice played normally");
                    DisplayDialogueOptions();
                }
                /*else if (_mSentences.Count == 1)
                {
                    //Ugly way of doing it but we'll see, maybe this works.
                    //Send an event for others to listen to informing us about what dialogue was just called.
                    //We would need to send it to the right dialogue
                   
                    
                }*/
                else
                {
                    branchChoiceDialogueBoxOpen = false;
                }
                
                //We then dequeue the sentences, names, voice lines, and the color of our text for the given dialogue.

                Debug.Log("Sentence dequeued manually");
                var sentence = _mSentences.Dequeue();
                var name = _mNames.Dequeue();
                var voiceLine = _mAudioClips.Dequeue();
                var textColor = _mColors.Dequeue();
                //If the enumerator is not null we stop it.
                if (typeSentence != null)
                {
                    StopCoroutine(typeSentence);
                }
                //We run the type sentence script on the sentences we have dequeued earlier.
                typeSentence = TypeSentence(sentence, name,textColor, voiceLine);
                //Then we run the coroutine.
                StartCoroutine(typeSentence);
                
                
            }
           
            
        }

        private void Update()
        {
            
            if (dialogueOpen != true) return;

            
            if (autoPlayDialogue)
            {
                AutoplayNextSentence();
            }
            else
            {
                if (useAutomaticDialogueSkip)
                {
                    if (dialogueSkipTimer > 0)
                    {
                        dialogueSkipTimer -= Time.deltaTime;
                
                    }
                    else
                    {
                        DisplayNextSentence();
                        dialogueSkipTimer = timeBeforeAutomaticSkip;

                    }
                }
            }
            
            
            
           
           
            //if (_audioSource.clip == null) return;
            //if (_audioSource.isPlaying != true)
            //{
                //DisplayNextSentence();
            //}
        }

        public void DisplayDialogueOptions()
        {
            scrollWindowBackground.SetActive(true);
            //List<DialogueContainer> containers = new List<DialogueContainer>();
            foreach (var branch in dialogueContainer.branches)
            {
                var instance = Instantiate(buttonPrefab, scrollContent.transform);
                instance.gameObject.SetActive(true);
                var text = instance.GetComponentInChildren<TMP_Text>();
                text.text = branch.name;
                instance.onClick.AddListener((() => OnChoiceButtonClicked(text)));
            }

            
            //DrawDialogueOptions();
            
            //buttonText.text = dialogueOptionNames[0];
            
            //buttonPrefab.transform.gameObject.SetActive(true);
            
            
            
        }
        
       

        public void DrawDialogueOptions()
        {
            dialogueOptionsAdded?.Invoke();
        }

        public void OnChoiceButtonClicked(TMP_Text text)
        {

            dialogueOptions.TryGetValue(text.text, out chosenDialogue);
             HideButtons();
             //TODO Fix this so it actually works with just one function call. Very hacky at the moment
             DisplayNextSentence();
             DisplayNextSentence();
        }

        public void OnChoiceButton2Clicked()
        {
            if (dialogueContainer.containerType == ContainerType.BranchingDialogue)
            {
                dialogueOptions.TryGetValue(dialogueOptionNames[1], out chosenDialogue);
            }
            
            HideButtons();
            //TODO Fix this so it actually works with just one function call. Very hacky at the moment
            DisplayNextSentence();
            DisplayNextSentence();
        }

        public void HideButtons()
        {

            foreach (Transform child in scrollContent.transform)
            {
                Destroy(child.gameObject);
            }
            scrollWindowBackground.gameObject.SetActive(false);
            //button1.transform.gameObject.SetActive(false);
            //button2.transform.gameObject.SetActive(false);
        }

        private IEnumerator DisplayDialogue()
        {


            while (true)
            {
                if (dialogues.Count != 0)
                {
                    Dialogue dialogue = dialogues.Dequeue();
                    if (autoPlayDialogue == false)
                    {
                        StartDialogue(dialogue);
                    }
                    else
                    {
                        StartAutoplayDialogue(dialogue);
                    }
                    
                }

                yield return new WaitUntil(DialogueComplete);
            }
                    //Debug.Log(dialogues.Count);
                    

                /*foreach (var dialogue in dialogues)
                {
                    StartDialogue(dialogue);
                    yield return new WaitUntil(DialogueComplete);
                }*/
        }

        private bool DialogueComplete()
        {
            if (dialogueOpen == false)
            {
                return true;
            }
            return false;
            
        }

        //Used to print out every letter in a dialogue in sequence.
        private IEnumerator TypeSentence (string sentence, string name,Color color, AudioClip voiceLine)
        {
            dialogueText.text = "";
            titleText.text = name;
            dialogueText.color = color;

            if (voiceLine != null)
            {
                //_audioSource.clip = voiceLine;
                //_audioSource.Play();
            }

            foreach (var letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(letterDelay*Time.deltaTime);
            }

        
        }
        
        

        public void EnqueueDialogue(DialogueContainer dialogueContainer)
        {
            
            this.dialogueContainer = dialogueContainer;
            currentContainerType = dialogueContainer.containerType;
            //Set the font here
            if (dialogueContainer.useCustomFontSize)
            {
                dialogueText.fontSize = dialogueContainer.fontScale;
            }
            else
            {
                dialogueText.fontSize = standardFontSize;
            }
            
            if (dialogueContainer.containerType == ContainerType.BranchingDialogue)
            {
                dialogueOptions = dialogueContainer.GetBranches();
            
                foreach (var branch in dialogueOptions)
                {
                    dialogueOptionNames.Add(branch.Key);
                }
                
            }

            if (dialogueContainer.containerType == ContainerType.DialogueWithStateSwapper)
            {
                chosenDialogue = dialogueContainer.dialogueStateSwapper; //dialogueContainer.
            }
           
            dialogues.Enqueue(dialogueContainer.dialogue);
            
            dialogueAdded = true;

        }

        private void EndDialogue()
        {
            useAutomaticDialogueSkip = false;
            //If the current container is using branches, swap to that one.
            if (dialogueContainer.containerType == ContainerType.BranchingDialogue && chosenDialogue.containerType != ContainerType.StateSwapper)
            {
                
                EnqueueDialogue(chosenDialogue);
            }
            else
            {
                if (chosenDialogue != null)
                {
                    if (chosenDialogue.containerType == ContainerType.StateSwapper)
                    {
                        chosenDialogue.SwapQuestState();
                    }
                    
                }
                
                
                
                dialogueOpen = false;
                animator.SetBool(IsOpen, false);
                //_audioSource.Stop();
                dialogueClosed?.Invoke();
                
                /*if (dialogueContainer.branches.Length == 0)
                {
                    Debug.Log("This was the last dialogue in the sequence");
                    reachedLastDialogueSO?.Invoke(dialogueContainer.dialogueSOLast);
                }*/
            }
            
            
           
            
            /*if (dialogues.Count != 0)
            {
                Dialogue dialogue = dialogues.Dequeue();
                StartDialogue(dialogue);
            }*/
           
        }

        public void CheckLastDialogueBox()
        {
            if (dialogueContainer.branches.Length == 0)
            {
                Debug.Log("This was the last dialogue in the sequence");
                reachedLastDialogueSO?.Invoke(dialogueContainer.dialogueSOLast);
            }
        }
    }
