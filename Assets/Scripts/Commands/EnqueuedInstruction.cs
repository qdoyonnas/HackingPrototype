using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnqueuedInstruction {

	NodeScript node;
	public NodeScript GetNode()
	{
		return node;
	}
	int sourceInstruction;
	public int GetSource()
	{
		return sourceInstruction;
	}

	public EnqueuedInstruction(NodeScript in_node, int source)
	{
		node = in_node;
		sourceInstruction = source;
	}

	public string Interpret()
	{
		return CommandsManager.GetProgramCommands().Interpret(this);
	}

	public void EnqueueNextInstruction(NodeScript node, string input, int sourceInstruction, bool success)
	{
		if( !success ) {
			return;
		}

		node.instructionQueue.Enqueue( new EnqueuedInstruction(node, sourceInstruction+1) );
	}

	public void EnqueueNextInstruction()
	{
		EnqueueNextInstruction(node, null, sourceInstruction, true);
	}
}