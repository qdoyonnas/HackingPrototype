using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminScript : PlayerScript {

	public override void Init(double in_id)
	{
		id = in_id;
		connectedNode = NodeManager.GetNode(0);
		SetStaticValues();
		GameManager.adminConsole = id;
	}

	protected override void SetStaticValues()
	{
		staticValues.Add("id", id);
		staticValues.Add("playerid", GameManager.playerConsole);
	}

	public override void CodeUpdate()
	{
		if ( commandQueue.Count > 0 ) {
            EnqueuedCommand command = commandQueue.Dequeue();
            connectedNode.commandQueue.Enqueue(command);
        }
	}
}