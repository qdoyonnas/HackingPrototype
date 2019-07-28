using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketManager
{
	public static List<PacketScript> packets = new List<PacketScript>();
	public static PacketCommandsScript commands = new PacketCommandsScript();

	public static void Init()
	{
		// Nothing for now
	}

	public static void CodeUpdate()
	{
		for( int i=packets.Count-1; i >= 0; i-- ) {
			packets[i].CodeUpdate();
		}
	}

	public static string CreatePacket(NodeScript source, NodeScript target, string contents)
	{
		// Create and validate packet
		PacketScript script = new PacketScript(source, target, contents);
		string errors = commands.Interpret(source, script);
		if( errors != null ) {
			return errors;
		}
		packets.Add(script);

		PacketGraphicManager.DisplayPacket(source, target);

		return null;
	}
}