using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketScript
{
	NodeScript sourceNode;
	public NodeScript GetSource()
	{
		return sourceNode;
	}
	NodeScript targetNode;
	public NodeScript GetTarget()
	{
		return targetNode;
	}

	string contents;
	public string GetContents()
	{
		return contents;
	}
	EnqueuedCommand command;
	public void SetCommand( EnqueuedCommand cmd )
	{
		command = cmd;
	}
	float arrivalTime;

	public PacketScript(NodeScript source, NodeScript target, string input)
	{
		// Get Data
		sourceNode = source;
		targetNode = target;
		contents = input;
		arrivalTime = Time.time + GameManager.gameOptions.packetTravelTime;
	}

	public void CodeUpdate()
	{
		if( Time.time >= arrivalTime ) {
			if( targetNode != null ) {
				Deliver();
			}
			PacketManager.packets.Remove(this);
		}
	}

	void Deliver()
	{
		command.sourceInstruction = targetNode.portPointer;
		targetNode.commandQueue.Enqueue(command);
	}
}