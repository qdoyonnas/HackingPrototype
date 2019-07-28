using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleScript : Interpreter
{
	[NonSerialized]
	public PlayerScript player;
	public Dictionary<string, string> aliases = new Dictionary<string, string>();

	UnityEngine.Object windowPrefab;
	List<WindowScript> windows = new List<WindowScript>();

	public ConsoleScript()
		:base() 
	{
		windowPrefab = Resources.Load("Prefabs/Window");
	}

	protected override void SetupCommands()
	{
		base.SetupCommands();

		// Camera Focus Commands
		commands.Add("spin", SetFocusSpinCommand);
		commands.Add("rotate", SetFocusRotateCommand);
		commands.Add("focustime", SetFocusTimeCommand);
		commands.Add("zoom", ZoomCommand);
		commands.Add("reset", ResetCommand);

		// Console Commands
		commands.Add("echo", EchoCommand);
		commands.Add("help", HelpCommand);
		commands.Add("clear", ClearConsole);
		commands.Add("alias", AliasCommand);
		commands.Add("forget", ForgetCommand);

		// File Commands
		//commands.Add("download", DownloadCommand);
		//commands.Add("upload", UploadCommand);
		//commands.Add("savekit", SaveKitCommand);

		// Info Commands
		commands.Add("info", InfoCommand);
		commands.Add("readmemory", ReadMemoryCommand);
		commands.Add("readlog", ReadLogCommand);
		commands.Add("hidenodes", HideNodesCommand);
		commands.Add("window", WindowCommand);

		// Net Commands
		commands.Add("connect", ConnectCommand);
		//commands.Add("connectforce", ConnectForceCommand);

		// Admin Mode
		commands.Add("adminmode", AdminModeCommand);

		// Preset Aliases
		aliases.Add("rdm", "readmemory");
		aliases.Add("rdl", "readlog");
	}

	protected int CreateWindow(Vector2 pos)
	{
		GameObject window = GameObject.Instantiate(windowPrefab) as GameObject;
		window.transform.SetParent(player.transform, false);

		RectTransform playerRect = player.GetComponent<RectTransform>();
		RectTransform windowRect = window.GetComponent<RectTransform>();
		windowRect.anchoredPosition = new Vector3(playerRect.rect.width * pos[0], playerRect.rect.height * pos[1], 0);

		WindowScript script = window.GetComponent<WindowScript>();
		windows.Add(script);

		return windows.Count-1;
	}
	protected int CreateWindow(float x, float y)
	{
		return CreateWindow(new Vector2(x, y));
	}

	public string Interpret( PlayerScript in_player, string input, out bool success )
	{
		player = in_player;
		localNode = player.connectedNode;
		success = false;

		// Divide input
		Regex pattern = new Regex(@"^([^ ]+)(?:\s+(.+))?");
		Match match = pattern.Match(input);
        if( !match.Success ) {
            return "'" + input + "' invalid console command format";
        }
		string command = match.Groups[1].Value;
		string parameters = match.Groups[2].Value;

		// Interpret command across all command lists (if node based -> compare for enqueing)
		Command cmd = null;
		string alias = null;
		string cmd_lower = command.ToLower();
		if( !commands.TryGetValue(cmd_lower, out cmd)
				&& !aliases.TryGetValue(cmd_lower, out alias) ) {
			return "'" + command + "' in '" + input + "' command not found";
		}	

		if( alias != null ) {
			if( parameters.Length == 0 ) {
				return Interpret(in_player, aliases[cmd_lower], out success);
			} else {
				return Interpret(in_player, aliases[cmd_lower] + " " + parameters, out success);
			}
		}

		string error = EvalStatic(parameters, out parameters);
		if( error != null ) {
			return error;
		}
		/*error = EvalLabels(parameters, out parameters);
        if( error != null ) {
            return error;
        }*/

		// Run command
		string output = cmd(parameters, out success);

		// Print output
		player = null;
		success = true;
		return output;
	}

	protected string EvalStatic( string input, out string result )
    {
        result = input;

        // Evaluate Labels
        Regex labelPattern = new Regex(@"\|(" + P.DATA + @")\|");
        MatchCollection matches = labelPattern.Matches(input);
        foreach (Match label in matches)
        {
            // Validate label
            string label_lower = label.Groups[1].Value.ToLower();
			double value = 0;
			if( !player.GetStatic().TryGetValue(label_lower, out value) ) {
				return "Static address '" + label.Value + "' not found";
			}

            // Substitute label
            int pos = input.IndexOf(label.Value);
            input = input.Remove(pos, label.Value.Length);
            input = input.Insert(pos, AlphaNumeral.DblToString(value));
        }

        result = input;
        return null;
	}

	protected string EchoCommand( string input, out bool success )
	{
		success = true;
		return input;
	}
	protected string ClearConsole( string input, out bool success )
	{
		player.ClearConsole();
		success = true;
		return "";
	}

	protected string AliasCommand( string input, out bool success )
	{
		success = false;

		// Show list of aliases
		if( input.Length == 0 ) {
			string output = 
				"Aliases\n" +
				"----------------------\n";
			foreach( string key in aliases.Keys ) {
				output += key + " : " + aliases[key] + "\n";
			}

			success = true;
			return output;
		}

		// Evaluate Input
		Regex pattern = new Regex(@"^("+P.DATA+@")\s+(.+)");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'" + input + "' invalid alias format";
		}

		// Validate alias
		string alias = match.Groups[1].Value;
		if( alias.Length < 2 ) {
			return "'" + alias + "' alias too short (min 2)";
		}
		string cmd;
		if( aliases.TryGetValue(alias, out cmd) ) {
			return ("Alias '" + alias + "' already exists");
		}

		// Add new Command
		string command = match.Groups[2].Value;
		aliases.Add(alias, command);

		success = true;
		return ("Alias added: '" + alias + "' for '" + command + "'");
	}
	protected string ForgetCommand( string input, out bool success )
	{
		success = false;

		string command;
		if( !aliases.TryGetValue(input, out command) ) {
			return("'" + input + "' alias not found");
		}

		aliases.Remove(input);

		success = true;
		return ("Alias '" + input + "' removed");
	}

	protected virtual string HelpCommand( string input, out bool success )
	{
		success = false;

		// Header
		string output = 
			"Help\n" +
			"---------------------------------------------------\n";

		// Display table of help contents
		if( input.Length == 0 ) {
			output += 
				"\nTopics\n" +
				"------------\n";
			for( int i=0; i<Descriptions.topicsKeys.Length; i++ ) {
				output += Descriptions.topicsKeys[i] + "\n";
			}

			output +=
				"\nConsole Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.consolesKeys.Length; i++ ) {
				output += Descriptions.consolesKeys[i] + "\n";
			}

			output +=
				"\nNode Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.commandsKeys.Length; i++ ) {
				output += Descriptions.commandsKeys[i] + "\n";
			}

			output +=
				"\nMath Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.mathsKeys.Length; i++ ) {
				output += Descriptions.mathsKeys[i] + "\n";
			}

            output +=
                "\nDecision Commands\n" +
                "------------\n";
            for (int i = 0; i < Descriptions.decisionsKeys.Length; i++)
            {
                output += Descriptions.decisionsKeys[i] + "\n";
            }

            output += "\nType 'help <topic/command>' for detailed help";
			success = true;
			return output;
		}

		// Display topic/command help
		output += "<> - parameter [] - optional !! - root required\n\n";
		// Check Topics
		if( Array.IndexOf<string>(Descriptions.topicsKeys, input) != -1 ) {
			output += Descriptions.topics(input);

			success = true;
			return output;
		}
		// Check Consoles
		if( Array.IndexOf<string>(Descriptions.consolesKeys, input) != -1 ) {
			output += Descriptions.consoles(input);

			success = true;
			return output;
		}
		// Check Commands
		if( Array.IndexOf<string>(Descriptions.commandsKeys, input) != -1 ) {
			output += Descriptions.commands(input);

			success = true;
			return output;
		}
		// Check Maths
		if( Array.IndexOf<string>(Descriptions.mathsKeys, input) != -1 ) {
			output += Descriptions.maths(input);

			success = true;
			return output;
		}
        // Check Decisions
        if (Array.IndexOf<string>(Descriptions.decisionsKeys, input) != -1)
        {
            output += Descriptions.decisions(input);

            success = true;
            return output;
        }

        return "Key '" + input + "' not found in help contents";
	}
	protected virtual string InfoCommand( string input, out bool success )
	{
		success = true;
        string output =
            "Console Info:\n" +
            "\tId: " + player.GetIdString() +
            "\n\tTime Since Last Read: " + (Time.time - player.timeLastRead) +
            "\n" +
            "\nLocal Node Info:\n" +
		    "\tAddress: " + player.connectedNode.GetAddressString() +
		    "\n\tPort: " + player.connectedNode.portPointer;

        return output;
	}

	protected string SetFocusSpinCommand( string input, out bool success )
	{
		success = false;

		// Short hand zero rotate
		if( input.Length == 0 ) {
			player.cameraFocus.spin = Vector3.zero;
			return ("Camera spin stopped");
		}

		Regex pattern = new Regex(@"\G("+P.FLOAT+@")\s*");
		MatchCollection matches = pattern.Matches(input);
		if( matches.Count == 0 ) {
			return "'" + input + "' invalid rotation format";
		}

		float[] values = { 0, 0, 0 };
		int i = 0;
		foreach( Match match in matches ) {
			string number = match.Groups[1].Value;
			int result;
			if( !AlphaNumeral.StringToInt(number, out result) ) {
				return "'" + result + "' invalid number format";
			}

			values[i++] = result;
			if( i >= 3 ) {
				break;
			}
		}

		Vector3 newSpin = new Vector3(values[1]/100f, values[0]/100f, values[2]/100f);
		player.cameraFocus.spin = newSpin;
		success = true;
		return ("Camera spin set to: ( " + values[0] + " " + values[1] + " " + values[2] + " )");
	}
	protected string SetFocusRotateCommand( string input, out bool success )
	{
		success = false;

		Regex pattern = new Regex(@"\G("+P.FLOAT+@")\s*");
		MatchCollection matches = pattern.Matches(input);
		if( matches.Count == 0 ) {
			return "'" + input + "' invalid rotation format";
		}

		float[] values = { 0, 0, 0 };
		int i = 0;
		foreach( Match match in matches ) {
			string number = match.Groups[1].Value;
			int result;
			if( !AlphaNumeral.StringToInt(number, out result) ) {
				return "'" + result + "' invalid number format";
			}

			values[i++] = result;
			if( i >= 3 ) {
				break;
			}
		}

		Vector3 rotate = new Vector3(values[1], values[0], values[2]);
		player.cameraFocus.SetRotation(rotate);
		success = true;
		return "Rotating camera...";
	}
	protected string SetFocusTimeCommand( string input, out bool success )
	{
		success = false;

		// Evaluate Input
		int result;
		if( !AlphaNumeral.StringToInt(input, out result) ) {
			return ("'" + input + "' not a valid number format");
		}
		
		// Set Speed
		player.cameraFocus.defaultMoveDuration = result;
		success = true;
		return ("Focus time set to: " + input);
	}
	protected string ZoomCommand( string input, out bool success )
	{
		success = false;

		int result;
		if( !AlphaNumeral.StringToInt(input, out result) ) {
			return ("'" + input + "' invalid number format");
		}

		player.cameraFocus.SetDistance(result);
		success = true;
		return ("Camera Zoom set to " + input);
	}
	protected string ResetCommand( string input, out bool success )
	{
		success = true;

		NodeGraphicScript graphic = NodeGraphicManager.GetGraphic(localNode);
		graphic.HideDisplay();
		graphic.ShowMemory(false);
		player.cameraFocus.Offset(Vector3.zero);
		player.cameraFocus.SetDistance(4);

		return "Resetting view...";
	}

	protected string ReadMemoryCommand( string input, out bool success )
	{
		success = false;

		if( input.Length != 0 ) {
			if( input.ToLower() == "static" ) {
				return "Static not implemented...";
				/*for( int i = GameManager.gameOptions.nodeMemoryLength; i < player.connectedNode.GetMemoryLength(); i++ ) {
					Memory memory = player.connectedNode.GetMemory(i);
					output += FormatMemory(i, memory, false);
				}*/
			} else {
				int index;
				int subIndex;
				string error = localNode.ParseMemoryIndex(input, out index, out subIndex);
				if( error != null ) {
					return error;
				}
				NodeGraphicScript graphic = NodeGraphicManager.GetGraphic(localNode);
				graphic.RotateMemory(index);
			}
		} else {
			NodeGraphicScript graphic = NodeGraphicManager.GetGraphic(localNode);
			graphic.SetRotationSpeed(graphic.memoryRotationSpeed);
		}
		NodeGraphicManager.GetGraphic(localNode).ShowMemory(true);
		player.cameraFocus.Offset(new Vector3(1.5f,0,0));
		
		success = true;
		return "Opening Memory...";
	}
	
	protected string ReadLogCommand( string input, out bool success )
	{
		success = false;

		/* XXX: Left for later use for log filtering
		Regex pattern = new Regex(@"");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "ReadLog invalid format";
		}*/

		LogDetails detail;
		switch( input ) {
			case "all":
				detail = LogDetails.ALL;
				break;
			case "command":
				detail = LogDetails.COMMAND;
				break;
			case "":
			case "response":
				detail = LogDetails.RESPONSE;
				break;
			default:
				return "ReadLog invalid format";
		}

		NodeGraphicScript graphic = NodeGraphicManager.GetGraphic(localNode);
		graphic.DisplayLog(detail);
		
		player.cameraFocus.Offset(new Vector3(-0.5f, 0, 2.5f));

		graphic.ShowMemory(false);
		success = true;
		return "Opening log...";
	}

	protected string WindowCommand( string input, out bool success )
	{
		success = false;

		int windowIndx;
		if( !int.TryParse(input, out windowIndx) ) {
			return "'" + input + "' invalid number format";
		}

		// XXX: until i decide how to handle windows
		windowIndx = 0;

		WindowScript window = windows[windowIndx];

		if( window != null ) {
			windows.RemoveAt(windowIndx);
			GameObject.Destroy(window.gameObject);
		} else {
			CreateWindow(-0.25f, 0.25f);
		}

		success = true;
		return "Window";
	}

	protected string HideNodesCommand( string input, out bool success )
	{
		success = true;
		NodeGraphicManager.HideAll();

		return "Hiding Nodes...";
	}

	/*protected string DownloadCommand( string input, out bool success )
	{
		success = false;

		// Parse
		Regex pattern = new Regex(@"^(" + P.INT + @")\s+(" + P.INT + @")\s+(.*)");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'" + input + "' invalid Download command format";
		}

		// Validate
		string index = match.Groups[1].Value;
		int i_index;
		int subIndex;
		string error = localNode.ParseMemoryIndex(index, out i_index, out subIndex);
		if( error != null ) {
			return error;
		}

		string count = match.Groups[2].Value;
		int i_count;
		bool b = AlphaNumeral.StringToInt(count, out i_count);
		if( !b ) {
			return "'" + count + "' in '" + input + "' invalid number format";
		}

		string fileName = match.Groups[3].Value;

		// Gather Data
		string[] data = new string[i_count];
		for( int i=0; i < i_count; i++ ) {
			int idx = (i_index + i) % GameManager.gameOptions.nodeMemoryLength;
			Memory memory = localNode.GetMemory(idx);
			data[i] = "[" + localNode.GetLabel(idx) + "]" + " " + (memory.isData?"d":"i") + " " + memory.contents;
		}

		// Write to file
		error = ClusterManager.SaveProgram(fileName, data);
		if( error != null ) {
			return error;
		}

		success = true;
		return "Downloading program...";
	}
	protected string UploadCommand( string input, out bool success )
	{
		success = false;

		// Parse
		Regex pattern = new Regex(@"^(" + P.INDEX + @")\s+(.*)");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'" + input + "' invalid Upload command format";
		}

		// Validate
		string index = match.Groups[1].Value;
		int i_index;
		string error = localNode.ParseMemoryIndex(index, out i_index);
		if( error != null ) {
			return error;
		}

		string fileName = match.Groups[2].Value;

		// Gather Data
		string[] data;
		error = ClusterManager.LoadProgram(fileName, out data);
		if( error != null ) {
			return error;
		}

		// Parse data lines
		Regex dataPattern = new Regex(@"^[(" + P.LABEL + @")]\s+(" + P.INDEX + @")\s+(.*)");
		for( int i=0; i < data.Length; i++ ) {
			Match dataMatch = dataPattern.Match(data[i]);
			if( !dataMatch.Success ) {
				return "'" + data[i] + "' invalid data format";
			}
			string dataLabel = dataMatch.Groups[1].Value;
			string dataIndex = dataMatch.Groups[2].Value;
			string dataContent = dataMatch.Groups[3].Value;

			// Calc index
			int i_dataIndex;
			if( !int.TryParse(dataIndex, out i_dataIndex) ) {
				return "'" + dataIndex + "' in '" + data[i] + "' invalid number format";
			}
			int idx = (i_index + i_dataIndex) % GameManager.gameOptions.nodeMemoryLength;

			// Apply data to node memory
			localNode.SetMemory(idx, dataContent);
			localNode.AddLabel(idx, dataLabel);
		}

		success = true;
		return "Uploading program...";
	}
	protected string SaveKitCommand( string input, out bool success )
	{
		success = false;

		// Collect all Memory
		string[] data = new string[GameManager.gameOptions.nodeMemoryLength];
		for( int i=0; i < data.Length; i++ ) {
			Memory memory = localNode.GetMemory(i);
			data[i] = "[" + localNode.GetLabel(i) + "]" + " " + (memory.isData?"d":"i") + " " + memory.contents;
		}

		// Write to file
		string error = ClusterManager.SaveProgram(input, data);
		if( error != null ) {
			return error;
		}

		success = true;
		return "Saving kit...";
	}*/

	protected string Connect( string input, bool safe, out bool success )
	{
		success = false;
		
		// Parse
		Regex pattern = new Regex(@"^(" + P.DATA + @")\s+(" + P.INT + @")");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'" + input + "' invalid connect format";
		}

		// Format
		// XXX: Add option to force Connect ignoring safe mode
		string address = match.Groups[1].Value;
		double d_address;
		if( !AlphaNumeral.StringToDbl(address, out d_address) ) {
			return "'" + address + "' in '" + input + "' invalid number format";
		}

		if( safe ) {
			if( !NodeGraphicManager.NodeIsVisible(d_address) ) {
				return "Safe Mode: Unknown Node. Use connectforce to force.";
			}
		}

		string index = match.Groups[2].Value;

		// Enqueue send exec command to new node to activate the new readconsole
		string sendInput = address + " exec " + index;
		player.commandQueue.Enqueue( new EnqueuedCommand(SendAction, sendInput, 0, ConnectCallback) );

		success = true;
		return "Connecting...";
	}
	protected string ConnectCommand( string input, out bool success )
	{
		return Connect( input, true, out success );
	}
	protected string ConnectForceCommand( string input, out bool success )
	{
		return Connect( input, false, out success );
	}
	protected void ConnectCallback(NodeScript node, string input, int sourceInstruction, bool success)
	{
		if( !success ) {
			player.PrintToConsole("Connection attempt failed to send");
			return;
		}
		
		Regex pattern = new Regex(@"^(" + P.DATA + @")\s+(" + P.INT + @")");
		Match match = pattern.Match(input);

		// Format
		// XXX: Add option to force Connect ignoring safe mode
		string address = match.Groups[1].Value;
		double d_address;
		AlphaNumeral.StringToDbl(address, out d_address);

		NodeScript targetNode = NodeManager.GetNode(d_address, true);
		/* XXX:
		if( targetNode == null ) {
			player.connectedNode = null;
			PacketGraphicScript packet = PacketGraphicManager.GetPacket();
			player.cameraFocus.MoveTo();
		}*/

		// Connect to new Node
		player.connectedNode = targetNode;

		// Camera pan to new Node
		player.cameraFocus.MoveTo(NodeGraphicManager.GetGraphic(targetNode),
							GameManager.gameOptions.packetTravelTime);
	}

	protected virtual string AdminModeCommand( string input, out bool success )
	{
		success = false;
		GameManager.activeConsole = GameManager.adminConsole;
		return "";
	}

	protected override string LogCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(LogAction, input, 0, null));

        success = true;
		return "Writing log on local Node...";
	}
	protected override string LabelCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(LabelAction, input, 0, null));

        success = true;
		return "Applying label on local Node...";
	}
	protected override string WriteCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(WriteAction, input, 0, null));

        success = true;
		return "Writing to memory on local Node...";
	}
	protected override string SendCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(SendAction, input, 0, null));

        success = true;
		return "Sending from local node...";
	}
	protected override string SetPortCommand( string input, out bool success )
	{
		player.commandQueue.Enqueue(new EnqueuedCommand(SetPortAction, input, 0, null));

        success = true;
		return "Sending from local node...";
	}
	protected override string AddCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(AddAction, input, 0, null));

        success = true;
		return "Caluclating on local Node...";
	}
	protected override string ExecCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(ExecAction, input, 0, null));

        success = true;
		return "Executing program on local Node...";
	}
	protected override string SubCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(SubAction, input, 0, null));

        success = true;
		return "Caluclating on local Node...";
	}
	protected override string MulCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(MulAction, input, 0, null));

        success = true;
		return "Caluclating on local Node...";
	}
	protected override string DivCommand( string input, out bool success )
	{
        player.commandQueue.Enqueue(new EnqueuedCommand(DivAction, input, 0, null));

		success = true;
		return "Caluclating on local Node...";
	}
}
