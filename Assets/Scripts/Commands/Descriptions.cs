using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Descriptions
{
	public static readonly string[] topicsKeys = 
	{
		"commands",
		"console",
		"nodes",
		"packets",
		"programs",
		"san",
		"top"
	};
	public static string topics( string key )
	{
		switch( key ) {
			case "commands":
				return 
				"";
			case "console":
				return 
				"";
			case "nodes":
				return 
				"";
			case "packets":
				return 
				"";
			case "programs":
				return 
				"";
			case "san":
				return
				"";
			case "top":
				return
				"";
			default:
				return "Topic not found";
		}
	}

	public static readonly string[] commandsKeys =
	{
		"exec",
		"label",
		"log",
        "readconsole",
		"send",
        "write",
	};
	public static string commands( string key )
	{
		switch (key)
		{
			case "exec":
				return
				"exec <index>\n" +
				"	Attempt to execute the Instruction at <index>";
			case "label":
				return
				"!label <index> <label>!\n" +
				"	Set <label> as Label for memory <index>";
			case "log":
				return
				"log <string>\n" +
				"	Print <string> to the Node's log";
            case "readconsole":
                return
                "readconsole <id>\n" +
                "   Retrieve output from connected console with id <id> and run it on the node.\n" +
                "   If no console with that id is connected then the program will crash.\n" +
                "   If the console does not have a pending output the command will complete\n" +
                "   successfully but will not add anything to the node's command queue";
			case "send":
				return
				"send <address> <string>\n" +
				"	Sends package containing <string> to node <address>. Package will always send\n" +
				"	regardless of if address is valid";
			case "write":
				return
				"write !index! <string>\n" +
				"	Write <string> to memory <index>. If <string> is an instruction, identified\n" +
				"	by it containing ':' then it is saved to memory as instruction ignoring\n" +
				"	data limitations";
			default:
				return "Command not found";
		}
	}

	public static readonly string[] mathsKeys =
	{
		"add",
		"div",
		"mul",
		"sub"
	};
	public static string maths( string key )
	{
		switch( key )
		{
			case "add":
				return
				"add <number> <index>\n" +
				"	Add <number> to contents of memory <index> and store in memory <index>";
			case "div":
				return
				"div <number> <index>\n" +
				"	Divide the contents of memory <index> by <number> and store\n" +
				"	in memory <index>";
			case "mul":
				return
				"mul <number> <index>\n" +
				"	Multiply <number> with contents of memory <index> and store in\n" +
				"	memory <index>";
			case "sub":
				return
				"sub <number> <index>\n" +
				"	Substract <number> from contents of memory <index> and store in\n" +
				"	memory <index>";
			default:
				return "Math command not found";
		}
	}
	
	public static readonly string[] consolesKeys =
	{
		"alias",
		"adminmode",
		"clear",
		"connect",
		"connectforce",
		"download",
		"echo",
		"focustime", 
		"forget",
		"help",
		"hidenodes",
		"info",
		"readlog",
		"readmemory",
		"rotate",
		"spin",
		"upload",
		"window",
		"zoom"
	};
	public static string consoles( string key )
	{
		switch( key )
		{
			case "alias":
				return
				"alias [<alias_name> <command>]\n" +
				"	Add a new alias <alias_name> for the command <command>. Command is\n" +
				"	an unvalidated string meaning it can contain parameters. If\n" +
				"	parameters are ommitted then shows list of current aliases";
			case "adminmode":
				return
				"adminmode\n" +
				"	Toggle to Admin Console. (Place holder)";
			case "clear":
				return
				"clear\n" +
				"	Clears out the console's backlog";
			case "connect":
				return
				"connect <address> <index>\n" +
				"	Transfer connection of console to Node at <address>. The command\n" +
				"	disconnects the console from the current Node and sends a packet\n" +
				"	to the new Node to execute a program at memory <index>.\n" +
				"	the console will travel with this packet, so the packet must be\n" +
				"	used to activate the 'readconsole' instruction on the new Node.\n" +
				"\n" +
				"	Safe Mode: prevents connecting to an unkown (not displayed) Node,\n" +
				"	use 'connectforce' to bypass this behavior";
			case "connectforce":
				return
				"connectforce <address> <index>\n" +
				"	(not implemented)Same behavior as 'connect' but bypasses Safe Mode checks";
			case "download":
				return
				"download <index1> <count> <string>\n" +
				"	Store contents of <count> memory cells starting at <index1> to\n" +
				"	Console, as file named <string>.\n" +
				"	Use 'upload' to load from a file back to a Node";
			case "echo":
				return
				"echo <string>\n" +
				"	Print <string> to console's backlog";
			case "focustime":
				return
				"focustime <int>\n" +
				"	Set the camera's transition time for moving animations";
			case "forget":
				return
				"forget <alias_name>\n" +
				"	Remove alias <alias_name>";
			case "help":
				return
				"help [topic/command]\n" +
				"	Display help for [topic/command] or table of help contents if\n" +
				"	[topic/command] is ommitted";
			case "hidenodes":
				return
				"hidenodes\n" +
				"	Hides all currently visible Nodes";
			case "info":
				return
				"info\n" +
				"	Print the node's info to the console backlog";
			case "readlog":
				return
				"readlog [index]\n" +
				"	Open the node log window and display logs from [index] back\n" +
				"	and newer. Or, if [index] is ommitted, display all the entire node's log";
			case "readmemory":
				return
				"readmemory ['static'|index] [window]\n" +
				"	Show memory block of local node that contains memory <index>,\n" +
				"	if <index> is ommitted will show the first block in memory.\n" + 
				"	If the word 'static' is passed instead, the static memory of\n" +
				"	will be shown";
			case "rotate":
				return
				"rotate [int1] [int2] [int3]\n" +
				"	Rotate the camera around the focus point by the values provided.\n" +
				"	All values may be ommitted in which case that value is defaulted to 0.\n";
			case "spin":
				return
				"spin [int1] [int2] [int3]\n" +
				"	Set the camera's rotation speed mapped to horizontal,\n" +
				"	vertical and roll format. All values may be ommitted in which\n" +
				"	case that value is defaulted to 0";
			case "upload":
				return
				"upload <index> <string>\n" +
				"	Load contents of file <string> from Console into the local Node's\n" +
				"	memory starting at memory <index>";
			case "window":
				return
				"window <index>\n" +
				"	Window <index> (0-based in order of creation) is toggled on or off.\n";
			case "zoom":
				return
				"zoom <int>\n" +
				"	Set camera distance from focused Node";
			default:
				return "Console command not found";
		}
	}

	public static readonly string[] decisionsKeys =
	{
		"equal",
		"notequal",
		"greater",
		"greaterequal",
		"lesser",
		"lesserequal"
	};
	public static string decisions( string key )
	{
		switch( key ) {
			case "equal":
				return
				"equal <index1> <int> <target>\n" +
				"	Compare content of memory <index1> and <int> and if\n" +
				"	they are equal set next instruction index to <target>";
			case "notequal":
				return
                "notequal <index1> <int> <target>\n" +
				"	Compare content of memory <index1> and <int> and if\n" +
				"	they are not equal set next instruction index to <target>";
			case "greater":
				return
				"greater <index1> <int> <target>\n" +
				"	Check if content of memory <index1> is greater than <int>\n" +
				"	and if it is, set next instruction index to <target>";
			case "greaterequal":
				return
				"greaterequal <index1> <int> <target>\n" +
				"	Check if content of memory <index1> is greater than or equal to\n" +
				"	<int> and if it is, set next instruction index to <target>";
			case "lesser":
				return
				"lesser <index1> <int> <target>\n" +
				"	Check if content of memory <index1> is less than <int>\n" +
				"	and if it is, set next instruction index to <target>";
			case "lesserequal":
				return
				"lesserequal <index1> <int> <target>\n" +
				"	Check if content of memory <index1> is less than or equal to\n" +
				"	<int> and if it is, set next instruction index to <target>";
			default:
				return "Decision command not found";
		}
	}

	public static readonly string[] adminKeys =
	{
		"getactive",
		"getany",
		"getinactive",
		"getlost"
	};
	public static string admins( string key )
	{
		switch( key ) {
			case "getinactive":
				return 
				"getinactive\n" +
				"	Print random inactive Node address";
			case "getactive":
				return
				"getactive\n" +
				"	Print random active Node address";
			case "getany":
				return
				"getany\n" +
				"	Print random Node address";
			case "getlost":
				return
				"getlost\n" +
				"	Connect admin console to a random Node";
			default:
				return "Admin command not found";
		}
	}
}