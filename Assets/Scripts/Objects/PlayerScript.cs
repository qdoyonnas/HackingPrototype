using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerScript : MonoBehaviour
{
	// STATIC
	public static double GenerateId()
	{
		double id;

		do {
			id = AlphaNumeral.RandomDouble(0, int.MaxValue);
		} while( GameManager.GetConsoles().ContainsKey(id) );

		return id;
	}
	
	// DYNAMIC
	public CameraFocusScript cameraFocus;
	public int commandMemoryLength = 100;
	[SerializeField]
	bool _isVisible = true;
	public bool isVisible {
		get {
			return _isVisible;
		}
		set {
			_isVisible = value;
			ShowRender(_isVisible);
		}
	}

    protected double id;
    public double GetId()
    {
        return id;
    }
	public string GetIdString()
	{
		return AlphaNumeral.DblToString(id);
	}
	
	NodeScript _connectedNode;
	public NodeScript connectedNode {
		get {
			return _connectedNode;
		}
		set {
			_connectedNode = value;
		}
	}

	WindowScript console;
	Text consoleText;
	InputField inputField;

	[NonSerialized]
	List<string> commandMemory = new List<string>();
	int memoryIndex = 0;

    public Queue<EnqueuedCommand> commandQueue = new Queue<EnqueuedCommand>();
	[NonSerialized]
    public float timeLastRead = 0;

	protected Dictionary<string, double> staticValues = new Dictionary<string, double>();
	public Dictionary<string, double> GetStatic()
	{
		return staticValues;
	}

    bool anyKeyLatch = false;

	public virtual void Init(double in_id)
	{
		id = in_id;

        connectedNode = NodeManager.CreateNode();
        int idx = UnityEngine.Random.Range(0, GameManager.gameOptions.nodeMemoryLength);
		
        MemoryBlock memory = connectedNode.GetMemory(idx);
		memory.SetContent(0, "READCON");
		memory.SetContent(1, id);
		
		memory = connectedNode.GetMemory(idx+1);
		memory.SetContent(0,"JMP");
		memory.SetContent(1, idx);

        connectedNode.instructionQueue.Enqueue( new EnqueuedInstruction(connectedNode, idx) );
        connectedNode.AddLabel(idx, "READC");

		NodeGraphicScript graphic = NodeGraphicManager.DisplayNode(Vector3.zero, connectedNode);
		cameraFocus.MoveTo(graphic);

		SetStaticValues();

		// XXX: Temporary code for local admin + non-admin
		GameManager.playerConsole = id;
		GameManager.activeConsole = id;
	}

	protected virtual void SetStaticValues()
	{
		staticValues.Add("id", id);
	}

	void Start()
	{
		// Console
		console = gameObject.transform.Find("Console").GetComponent<WindowScript>();
		if( !console ) {
			Debug.LogError("Player could not find Console");
		}
		consoleText = console.transform.Find("TextScroll/TextField").GetComponent<Text>() as Text;
		if( !consoleText ) {
			Debug.LogError("Console did not find TextField!");
		}
		inputField = console.transform.Find("InputField").GetComponent<InputField>() as InputField;
		if( !inputField ) {
			Debug.LogError("Console did not find InputField!");
		} else {
			inputField.onEndEdit.AddListener(delegate(string input) {
				if ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ) {
					if( inputField.text.Length > 0 ) {
						OnInputSubmit(input);
						anyKeyLatch = true;
					}
				}
			});
		}

		ShowRender(isVisible);

		// Focus input field
		if( isVisible ) {
			SelectInputField();
		}
	}

	// XXX: I really want to refactor this to be more readable
	public void KeepFocus()
	{
		if( !inputField.isActiveAndEnabled ) {
			return;
		}

		// Keep focus on Console
		if( !anyKeyLatch ) {
			if( Input.anyKey ) {
				anyKeyLatch = true;
				if( !Input.GetMouseButton(0) 
						&& !Input.GetMouseButton(1)
						&& !Input.GetMouseButton(2) ) {

					if( Input.GetKey(KeyCode.Escape) ) {
						HideAll();
					} else if( Input.GetKey(KeyCode.Return)
									|| Input.GetKey(KeyCode.KeypadEnter) ) {
						// do nothing
					} else if( Input.GetKey(KeyCode.UpArrow) ) {
						if( memoryIndex > 0 ) {
							inputField.text = commandMemory[--memoryIndex];
							SelectInputField();
						}
					} else if( Input.GetKey(KeyCode.DownArrow) ) {
						if( memoryIndex < commandMemory.Count-1 ) {
							inputField.text = commandMemory[++memoryIndex];
							SelectInputField();
						} else {
							inputField.text = "";
							SelectInputField();
						}
					}

					else if( !inputField.isFocused ) {
						inputField.text += Input.inputString;
						SelectInputField();
					}
				}
			}
		} else {
			if( !Input.anyKey ) {
				anyKeyLatch = false;
			}
		}
	}

    public virtual void CodeUpdate()
    {
		// XXX: Add timeout logic

        /*if( Time.time >= timeLastRead + GameManager.gameOptions.timeOut 
				&& inputField.isActiveAndEnabled ) {
            PrintToConsole("---- TIMED OUT ----");
            inputField.enabled = false;
        }*/
    }

	void SelectInputField()
	{
		console.SetHide(false);
		inputField.Select();
		inputField.ActivateInputField();

		// Move caret to end (and deselect)
		// must be done next frame to work
		StartCoroutine( MoveTextEnd_NextFrame() );
	}

	IEnumerator MoveTextEnd_NextFrame()
	{
		yield return 0; // Skip first frame
		inputField.MoveTextEnd(false);
	}

	// Submit on input field sends to text field
	void OnInputSubmit( string input )
	{
		// Print command to console
		PrintToConsole("\n> " + input);

		// Interpret
		bool success;
		PrintToConsole( CommandsManager.GetConsoleCommands().Interpret(this, input, out success) );

		// Remember Command
		commandMemory.Add(input);
		memoryIndex = commandMemory.Count;
		while( commandMemory.Count > commandMemoryLength ) {
			commandMemory.RemoveAt(0);
		}

		// Reset inputField
		inputField.text = "";
	}

	public void PrintToConsole( string input )
	{
		if( input.Length == 0 ) {
			return;
		}

		console.SetHide(false);
		consoleText.text += "\n" + input;
	}

	public void ClearConsole()
	{
		console.SetHide(false);
		consoleText.text = "";
	}

	public void HideAll()
	{
		inputField.text = "";
		console.SetHide(true);
	}

	public void ShowRender(bool doShow)
	{
		console.gameObject.SetActive(doShow);

		if( doShow ) {
			SelectInputField();
		}
	}
}
