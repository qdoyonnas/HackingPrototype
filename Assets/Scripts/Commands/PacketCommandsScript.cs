using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketCommandsScript : Interpreter
{
	PacketScript packet;

	public PacketCommandsScript()
		:base() {}

	protected override void SetupCommands()
	{
		base.SetupCommands();
	}

	// XXX: Dare I say I do not like how I am handling Packets
	public string Interpret(NodeScript node, PacketScript in_packet)
	{
		localNode = node;
		packet = in_packet;

		// Divide input
		Regex pattern = new Regex(@"^([^ ]+)(?:\s+(.+))?");
		Match match = pattern.Match(packet.GetContents());
        if( !match.Success ) {
            return "'" + packet.GetContents() + "' invalid command format";
        }
		string command = match.Groups[1].Value;
		string parameters = match.Groups[2].Value;

		// Interpret command across all command lists
		Command cmd = null;
		if( !commands.TryGetValue(command.ToLower(), out cmd) ) {
			return ("'" + command + "' in '" + packet.GetContents() + "' command not found");
		}

		/*if( packet.GetTarget() != null ) {
			string error = EvalLabels( parameters, out parameters, packet.GetTarget() );
			if( error != null ) {
				return error;
			}
		}*/

		// Run command
		bool success;
		string output = cmd(parameters, out success);
		if( !success ) {
			return output;
		}

		return null;
	}

	string MathCommand( string input, out bool success, string operation )
	{
		success = false;

		CommandAction action;
		// Save Revised command
		switch( operation ) {
			case "add":
				action = AddAction;
				break;
			case "sub":
				action = SubAction;
				break;
			case "mul":
				action = MulAction;
				break;
			case "div":
				action = DivAction;
				break;
			default:
				Debug.LogError("Packet MathCommand received invalid operation");
				return "Packet MathCommand received invalid operation";
		}
		packet.SetCommand( new EnqueuedCommand(action, "PORT " + input, 0, null) );

		success = true;
		return null;
	}
	protected override string AddCommand( string input, out bool success )
	{
		return MathCommand(input, out success, "add");
	}
	protected override string DivCommand( string input, out bool success )
	{
		return MathCommand(input, out success, "div");
	}
	protected override string ExecCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(ExecAction, input, 0, null) );
		return null;
	}
	protected override string LabelCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(LogAction, "Label: Permission Denied", 0, null) );
		return null;
	}
	protected override string LogCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(LogAction, input, 0, null) );
		return null;
	}
	protected override string MulCommand( string input, out bool success )
	{
		return MathCommand(input, out success, "mul");
	}
	protected override string SendCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(SendAction, input, 0, null) );
		return null;
	}
	protected override string SetPortCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(LogAction, "SetPort: Permission Denied", 0, null) );
		return null;
	}
	protected override string SubCommand( string input, out bool success )
	{
		return MathCommand(input, out success, "sub");
	}
	protected override string WriteCommand( string input, out bool success )
	{
		success = true;
		packet.SetCommand( new EnqueuedCommand(WriteAction, "PORT " + input, 0, null) );
		return null;
	}
}