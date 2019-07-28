using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LogDetails {
	ALL = 0,
	COMMAND,
	RESPONSE
}

public class LogEntry
{
	public string time;
	public string command;
	public string response;

	public LogEntry(string in_time, string in_command, string in_response)
	{
		time = in_time;
		command = in_command;
		response = in_response;
	}

	public string ToString(LogDetails detail)
	{
		string output = "[" + time + "] ";

		switch( detail ) {
			case LogDetails.ALL:
				output += command;
				if( response.Length > 0 ) {
					output += " : " + response;
				}
				break;
			case LogDetails.COMMAND:
				output += command;
				break;
			case LogDetails.RESPONSE:
				if( response.Length > 0 ) {
					output += response;
				} else {
					output = "";
				}
				break;
		}

		return output;
	}

	public override string ToString()
	{
		return ToString(LogDetails.ALL);
	}
}

public class NodeScript
{
	// PROPERTIES

	double address;
	public double GetAddress()
	{
		return address;
	}
	public string GetAddressString()
	{
		return AlphaNumeral.DblToString(address);
	}

	MemoryBlock[] memory;
	public MemoryBlock GetMemory(int idx)
	{
		return memory[idx];
	}

	string[] labels;
	public string GetLabel(int idx)
	{
		return labels[idx];
	}

	int _portPointer;
	public int portPointer {
		get {
			return _portPointer;
		}
		set {
			labels[_portPointer] = "";
			_portPointer = value;
			_portPointer = _portPointer % GameManager.gameOptions.nodeMemoryLength;
			labels[_portPointer] = "PORT";
		}
	}
	
	[NonSerialized]
	public LinkedList<LogEntry> log = new LinkedList<LogEntry>();
    public Queue<EnqueuedCommand> commandQueue = new Queue<EnqueuedCommand>();
	public Queue<EnqueuedInstruction> instructionQueue = new Queue<EnqueuedInstruction>();

	// CONSTRUCTOR

	public NodeScript(double in_id)
	{
		address = in_id;
		_portPointer = UnityEngine.Random.Range(0, GameManager.gameOptions.nodeMemoryLength-1);

		// XXX: Add logic for static Memory

		// Init memory
		//int staticMemory = 1;
		memory = new MemoryBlock[GameManager.gameOptions.nodeMemoryLength]; //+ staticMemory];
		for( int i=0; i < GameManager.gameOptions.nodeMemoryLength; i++ ) {
			memory[i] = new MemoryBlock();
		}

		//memory[GameManager.gameOptions.nodeMemoryLength+0] = new MemoryBlock(true, address);
		
		// Init labels
		labels = new string[GameManager.gameOptions.nodeMemoryLength]; //+ staticMemory];
		for( int i=0; i < GameManager.gameOptions.nodeMemoryLength; i++ ) {
			labels[i] = "";
		}

		//labels[GameManager.gameOptions.nodeMemoryLength+0] = "ADRS";
		//labels[portPointer] = "PORT";
	}

	// CODE STARTS HERE

	public void CodeUpdate()
	{
		// Run instruction Queue
		if( instructionQueue.Count > 0 ) {
			ReadNextInstruction();
		}

		// Run Command Queue
		if( commandQueue.Count > 0 ) {
			RunNextCommand();
		}

		NodeManager.callQueue.Enqueue(this);
	}

	void ReadNextInstruction()
	{
		EnqueuedInstruction nextInstruction = instructionQueue.Dequeue();
		string output = nextInstruction.Interpret();

		// Add log if error
		if( output != null ) {
			PrintToLog( "Program at " + nextInstruction.GetSource(), output );
		}
	}
	void RunNextCommand()
	{
		EnqueuedCommand nextCommand = commandQueue.Dequeue();

		string commandName;
		string output = nextCommand.Run(this, out commandName);

		PrintToLog( commandName + " " + nextCommand.GetInput(), output );
	}

	public void PrintToLog( string command, string response )
	{
		LogEntry entry = new LogEntry(DateTime.Now.ToLongTimeString(), command, response);
		log.AddLast(entry);

		while( log.Count > GameManager.gameOptions.maxLog ) {
			log.RemoveFirst();
		}
	}

	public string ReadLog(LogDetails detail)
	{
		string output = "";
		foreach( LogEntry entry in log ) {
			string str = entry.ToString(detail);
			if( str.Length > 0 ) {
				output += "\n" + str;
			}
		}

		return output;
	}
	
	public string ParseMemoryIndex( string ipt, int source, out int index, out int subIndex )
	{
        index = -1;
		subIndex = 0;
		
        Regex pattern = new Regex(@"^(" + P.INT + @")(?:[iI]([0-9]))?");
		Match match = pattern.Match(ipt);
        if( !match.Success ) {
            return "'" + ipt + "' invalid memory index format";
        }
		string s_index = match.Groups[1].Value;
		string s_subIndex = match.Groups[2].Value;

		// Validate SubIndex if there is one
		if( s_subIndex.Length != 0 && !int.TryParse(s_subIndex, out subIndex) ) {
			return( "'" + s_subIndex + "' invalid number format for sub-index");
		}

		// Check against labels // XXX: this could be faster by only checking against labels that exist
		for( int i=0; i < labels.Length; i++ ) {
			if( s_index.ToLower() == labels[i].ToLower() ) {
				index = i;
				break;
			}
		}

		// Check against static memory
		/*if( result >= GameManager.gameOptions.nodeMemoryLength 
				&& result < memory.Length ) {
			index = result;
			return null;
		}*/

		// Validate input as numeric index
		if ( index == -1 && !AlphaNumeral.StringToInt(s_index, out index) ) {
			return ("'" + s_index + "' invalid number format or label not found for index");
		}

		// Offset result from source if relative
		if( s_index[0] == '+' || s_index[0] == '-' ) {
			index = source + index;
		}

		// Wrap indexes around memory (not including static)
		if( index < 0 ) {
			index += GameManager.gameOptions.nodeMemoryLength;
		}
		index = index % GameManager.gameOptions.nodeMemoryLength;

		return null;
	}
	public string ParseMemoryIndex( string ipt, out int index, out int subIndex)
	{
		return ParseMemoryIndex(ipt, 0, out index, out subIndex);
	}

	public void AddLabel( int idx, string lbl )
	{
		if( idx == portPointer ) {
			return;
		}

		if( idx >= GameManager.gameOptions.nodeMemoryLength ) {
			Debug.LogError("Tried to overwrite label for static memory");
			return;
		}

		if( lbl.Length > GameManager.gameOptions.dataLength ) {
			Debug.LogError("Node was passed label above max data length");
			return;
		}

		labels[idx] = lbl.ToUpper();
	}

	public int GetMemoryLength()
	{
		return memory.Length;
	}

	public string SetMemory( int idx, int subIdx, double value)
	{
		if( idx == portPointer ) {
			portPointer++;
		}

		return memory[idx].SetContent(subIdx, value);
	}
	public string SetMemory( int idx, int subIdx, string content)
	{
		if( idx == portPointer ) {
			portPointer++;
		}

		return memory[idx].SetContent(subIdx, content);
	}

	public int[] GetInstructionIndexes()
	{
		int[] indexes = new int[instructionQueue.Count];
		int i = 0;
		foreach( EnqueuedInstruction instruct in instructionQueue ) {
			indexes[i] = instruct.GetSource();
			i++;
		}

		return indexes;
	}
	public bool IsMemoryQueued(int i)
	{
		bool query = false;

		foreach( EnqueuedInstruction instruct in instructionQueue ) {
			if( instruct.GetSource() == i ) {
				query = true;
				break;
			}
		}

		return query;
	}
}
