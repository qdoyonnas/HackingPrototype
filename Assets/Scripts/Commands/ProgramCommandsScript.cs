using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgramCommandsScript : Interpreter
{
	EnqueuedInstruction instruction;	

	public ProgramCommandsScript()
		: base() { }

	protected override void SetupCommands()
	{
		base.SetupCommands();

        commands.Add("readcon", ReadConsoleCommand);

        // Decisions
        commands.Add("equal", EqualCommand);
		commands.Add("notequal", NotEqualCommand);
		commands.Add("greater", GreaterCommand);
		commands.Add("gtrequal", GreaterEqualCommand);
		commands.Add("lesser", LesserCommand);
		commands.Add("lsrequal", LesserEqualCommand);
		commands.Add("jmp", JumpCommand);
	}

	public string Interpret(EnqueuedInstruction in_instruction)
	{
		instruction = in_instruction;
		MemoryBlock memoryBlock = instruction.GetNode().GetMemory(instruction.GetSource());
		double in_command = memoryBlock.GetCell(0).value;

		// Interpret command
		// XXX: Comparing using values instead of strings to guarantee interpretation
		//		Will require a better setup
		Command cmd = null;
		//Debug.Log("IN: " + in_command);
		foreach(string commandString in commands.Keys) {
			double commandValue;
			AlphaNumeral.StringToDbl(commandString, out commandValue);

			//Debug.Log("CMD: " + commandString);
			//Debug.Log("VAL: " + commandValue);

			if( in_command == commandValue ) {
				cmd = commands[commandString];
				break;
			}
		}
		if( cmd == null ) {
			return "Commands '" + memoryBlock.GetCell(0).content + "' not found";
		}

		// Gather 'input' string from MemoryBlock contents
		string input = "";
		for(int i=0; i < GameManager.gameOptions.memoryCellCount; i++) {
			input += memoryBlock.GetCell(i).content + " ";
		}

		bool success;
		string output = cmd(input, out success);

		instruction = null;
		return output;
	}

	private string ReadConsoleCommand( string input, out bool success )
	{

		success = false;

		string[] cells = input.Split(' ');

		double d_input;
		if( !AlphaNumeral.StringToDbl(cells[1], out d_input) ) {
			return "'" + cells[1] + "' invalid number format";
		}

        // Find player
        PlayerScript player = GameManager.FindPlayerAtNode(d_input, instruction.GetNode());
        if( player == null ) {
            return "Console '" + cells[1] + "' not found";
        }

		player.timeLastRead = Time.time;
        // Move command into queue
        if ( player.commandQueue.Count > 0 ) {
            EnqueuedCommand command = player.commandQueue.Dequeue();
			command.sourceInstruction = instruction.GetSource();
            instruction.GetNode().commandQueue.Enqueue(command);
        }

		// XXX: Unsure if 'readconsole' should be enqueued as a command itself first
		instruction.EnqueueNextInstruction();
		success = true;
		return null;
	}

	string CompareCommand( string input, out bool success, string comparison )
	{
		// Note - this will return success=false on errors and on comparison = true results
		//		only a non-error comparison = false will return success=true
		success = false;

        // Parse
        Regex pattern = new Regex(@"^("+P.DATA+@")\s+("+P.DATA+@")\s+("+P.DATA+@")");
        Match match = pattern.Match(input);
        if( !match.Success ) {
            return "'" + input + "' invalid compare command format";
        }
        string index1 = match.Groups[1].Value;
        string index2 = match.Groups[2].Value;
        string target = match.Groups[3].Value;

        // Validate
        int i_index1;
		int index1_subIndex;
        string error = instruction.GetNode().ParseMemoryIndex(index1, instruction.GetSource(), out i_index1, out index1_subIndex);
        if( error != null ) {
            return error;
        }
		double value1 = instruction.GetNode().GetMemory(i_index1).GetCell(index1_subIndex).value;

        int i_index2;
		int index2_subIndex;
        error = instruction.GetNode().ParseMemoryIndex(index2, instruction.GetSource(), out i_index2, out index2_subIndex);
        if( error != null ) {
            return error;
        }
		double value2 = instruction.GetNode().GetMemory(i_index2).GetCell(index2_subIndex).value;

        int i_target;
		int target_subIndex;
        error = instruction.GetNode().ParseMemoryIndex(target, instruction.GetSource(), out i_target, out target_subIndex);
        if( error != null ) {
            return error;
        }

        // Compare
        bool compared;
        switch( comparison.ToLower() ) {
            case "equal":
                compared = value1 == value2;
                break;
            case "notequal":
                compared = value1 != value2;
                break;
            case "greater":
                compared = value1 > value2;
                break;
            case "greaterequal":
                compared = value1 >= value2;
                break;
            case "lesser":
                compared = value1 < value2;
                break;
            case "lesserequal":
                compared = value1 <= value2;
                break;
            default:
                Debug.LogError("Comparison command ran with unknown comparator " + comparison);
                return "Comparison command ran with unknown comparator " + comparison;
        }
		// XXX: This needs to immediatedly run the following instruction 
		//		(every game update a command should be enqueued)
        if( compared ) {
            instruction.GetNode().instructionQueue.Enqueue( new EnqueuedInstruction(instruction.GetNode(), i_target) );
        } else {
			instruction.EnqueueNextInstruction();
        }
        
        success = true;
		return "";
	}
	string EqualCommand( string input, out bool success )
    {
        return CompareCommand(input, out success, "equal");
    }
	string NotEqualCommand( string input, out bool success )
	{
		return CompareCommand(input, out success, "notequal");
	}
	string GreaterCommand( string input, out bool success )
	{
        return CompareCommand(input, out success, "greater");
    }
	string GreaterEqualCommand( string input, out bool success )
	{
        return CompareCommand(input, out success, "greaterequal");
    }
	string LesserCommand( string input, out bool success )
	{
        return CompareCommand(input, out success, "lesser");
    }
	string LesserEqualCommand( string input, out bool success )
	{
        return CompareCommand(input, out success, "lesserequal");
    }

	private string JumpCommand( string input, out bool success )
	{
		success = false;

		string[] cells = input.Split(' ');

		int index;
		int subIndex;
		string error = instruction.GetNode().ParseMemoryIndex(cells[1], out index, out subIndex);
		if( error != null ) {
			return error;
		}

		// XXX: This needs to immediatedly run the following instruction 
		//		(every game update a command should be enqueued)
		instruction.GetNode().instructionQueue.Enqueue(new EnqueuedInstruction(instruction.GetNode(), index));
		success = true;
		return null;
	}

	protected override string LogCommand( string input, out bool success )
	{
		success = false;

		string[] cells = input.Split(' ');

		int index;
		int subIndex;
		string error = instruction.GetNode().ParseMemoryIndex(cells[1], out index, out subIndex);
		if( error != null ) {
			return error;
		}
		MemoryBlock memory = instruction.GetNode().GetMemory(index);
		string content = memory.ToString();

		EnqueuedCommand command = new EnqueuedCommand(LogAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string LabelCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(LabelAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string WriteCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(WriteAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string SendCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(SendAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string SetPortCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(SetPortAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string ExecCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(ExecAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string AddCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(AddAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string SubCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(SubAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string MulCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(MulAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
	protected override string DivCommand( string input, out bool success )
	{
		string content = "";

		EnqueuedCommand command = new EnqueuedCommand(DivAction, content, instruction.GetSource(), instruction.EnqueueNextInstruction);
		instruction.GetNode().commandQueue.Enqueue(command);

		success = true;
		return "";
	}
}