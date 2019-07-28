using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnqueuedCommand {
	
	CommandAction command;
	string input;
	public string GetInput()
	{
		return input;
	}
	public int sourceInstruction;
	CallbackAction callback;

	public EnqueuedCommand(CommandAction cmd, string in_input, int source, CallbackAction clbk)
	{
		command = cmd;
		input = in_input;
		callback = clbk;
		sourceInstruction = source;
	}

	public string Run(NodeScript node, out string commandName)
	{
		bool success;
		string output = command(node, input, sourceInstruction, out success, out commandName);

		if( callback != null ) {
			callback(node, input, sourceInstruction, success);
		}

		return output;
	}
}