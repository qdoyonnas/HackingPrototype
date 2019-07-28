using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommandsManager
{
	static AdminConsoleScript adminCommands = new AdminConsoleScript();
	static ConsoleScript consoleCommands = new ConsoleScript();
	static PacketCommandsScript packetCommands = new PacketCommandsScript();
	static ProgramCommandsScript programCommands = new ProgramCommandsScript();

	public static AdminConsoleScript GetAdminCommands()
	{
		return adminCommands;
	}
	public static ConsoleScript GetConsoleCommands()
	{
		return consoleCommands;
	}
	public static PacketCommandsScript GetPacketCommands()
	{
		return packetCommands;
	}
	public static ProgramCommandsScript GetProgramCommands()
	{
		return programCommands;
	}
}