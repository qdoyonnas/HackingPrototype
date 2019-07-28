using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminConsoleScript : ConsoleScript
{
	public AdminConsoleScript()
		:base() {}

	protected override void SetupCommands()
	{
		base.SetupCommands();

		commands.Add("getactive", GetActiveCommand);
		commands.Add("getany", GetAnyCommand);
		commands.Add("getinactive", GetInactiveCommand);
		commands.Add("getlost", GetLostCommand);

		//commands.Add("loadkit", LoadKitCommand);
	}

	protected override string HelpCommand( string input, out bool success )
	{
		// XXX: Duplicate code!
		success = false;

		// Header
		string output = 
			"Help\n" +
			"---------------------------------------------------\n";

		// Display table of help contents
		if( input.Length == 0 ) {
			output += 
				"\nTopics\n" +
				"------------\n";
			for( int i=0; i<Descriptions.topicsKeys.Length; i++ ) {
				output += Descriptions.topicsKeys[i] + "\n";
			}

			output +=
				"\nConsole Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.consolesKeys.Length; i++ ) {
				output += Descriptions.consolesKeys[i] + "\n";
			}

			output +=
				"\nNode Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.commandsKeys.Length; i++ ) {
				output += Descriptions.commandsKeys[i] + "\n";
			}

			output +=
				"\nMath Commands\n" +
				"------------\n";
			for( int i=0; i<Descriptions.mathsKeys.Length; i++ ) {
				output += Descriptions.mathsKeys[i] + "\n";
			}

            output +=
                "\nDecision Commands\n" +
                "------------\n";
            for (int i = 0; i < Descriptions.decisionsKeys.Length; i++)
            {
                output += Descriptions.decisionsKeys[i] + "\n";
            }

			output +=
                "\nAdmin Commands\n" +
                "------------\n";
            for (int i = 0; i < Descriptions.adminKeys.Length; i++)
            {
                output += Descriptions.adminKeys[i] + "\n";
            }

            output += "\nType 'help <topic/command>' for detailed help";
			success = true;
			return output;
		}

		// Display topic/command help
		output += "<> - parameter [] - optional !! - root required\n\n";
		// Check Topics
		if( Array.IndexOf<string>(Descriptions.topicsKeys, input) != -1 ) {
			output += Descriptions.topics(input);

			success = true;
			return output;
		}
		// Check Consoles
		if( Array.IndexOf<string>(Descriptions.consolesKeys, input) != -1 ) {
			output += Descriptions.consoles(input);

			success = true;
			return output;
		}
		// Check Commands
		if( Array.IndexOf<string>(Descriptions.commandsKeys, input) != -1 ) {
			output += Descriptions.commands(input);

			success = true;
			return output;
		}
		// Check Maths
		if( Array.IndexOf<string>(Descriptions.mathsKeys, input) != -1 ) {
			output += Descriptions.maths(input);

			success = true;
			return output;
		}
        // Check Decisions
        if (Array.IndexOf<string>(Descriptions.decisionsKeys, input) != -1)
        {
            output += Descriptions.decisions(input);

            success = true;
            return output;
        }
		// Check Admins
        if (Array.IndexOf<string>(Descriptions.adminKeys, input) != -1)
        {
            output += Descriptions.admins(input);

            success = true;
            return output;
        }

        return "Key '" + input + "' not found in help contents";
	}
	protected override string InfoCommand( string input, out bool success )
	{
		success = true;
        string output =
            "Console Info:\n" +
			"\tADMIN\n" +
            "\tId: " + player.GetId() +
            "\n\tTime Since Last Read: " + (Time.time - player.timeLastRead) +
            "\n" +
            "\nLocal Node Info:\n" +
		    "\tAddress: " + player.connectedNode.GetAddress() +
		    "\n\tPort: " + player.connectedNode.portPointer;

        return output;
	}
	protected override string AdminModeCommand( string input, out bool success )
	{
		success = true;
		GameManager.activeConsole = GameManager.playerConsole;
		return "";
	}

	string GetLostCommand( string input, out bool success )
	{
		double address = NodeManager.GetRandomAddress();

		NodeGraphicManager.DisplayNode(NodeGraphicManager.GetGraphic(player.connectedNode).transform.position, address);

		return Connect(address.ToString(), true, out success);
	}

	string GetInactiveCommand( string input, out bool success )
	{
		success = true;
		return AlphaNumeral.DblToString(NodeManager.GetInactiveAddress());
	}
	string GetAnyCommand( string input, out bool success )
	{
		success = true;
		return AlphaNumeral.DblToString(NodeManager.GetRandomAddress());
	}
	string GetActiveCommand( string input, out bool success )
	{
		success = true;
		// XXX: consider moving logic to NodeManager
		Dictionary<double, NodeScript> nodes = NodeManager.GetNodes();

		double address = 0;

		int rand = UnityEngine.Random.Range(0, nodes.Count);
		int i = 0;
		foreach( KeyValuePair<double, NodeScript> nodePair in nodes ) {
			address = nodePair.Key;
			if( i == rand ) {
				break;
			}
			i++;
		}

		return AlphaNumeral.DblToString(address);
	}

	/*string LoadKitCommand( string input, out bool success )
	{
		success = false;
		
		// Gather Data
		string[] data;
		string error = ClusterManager.LoadProgram(input, out data);
		if( error != null ) {
			return error;
		}

		// Parse data lines
		Regex dataPattern = new Regex(@"^\[(" + P.DATA + @")?\]\s+([di])\s+(.*)");
		for( int i=0; i < data.Length; i++ ) {
			Match dataMatch = dataPattern.Match(data[i]);
			if( !dataMatch.Success ) {
				return "'" + data[i] + "' invalid data format";
			}
			string dataLabel = dataMatch.Groups[1].Value;
			string dataType = dataMatch.Groups[2].Value;
			string dataContent = dataMatch.Groups[3].Value;

			bool isData = dataType == "d";

			// Apply data to node memory
			if( isData ) {
				bool isBase26 = AlphaNumeral.IsStringBase26(dataContent);

				double d_content;
				if( !AlphaNumeral.StringToDbl(dataContent, out d_content) ) {
					return "'" + dataContent + "' in '" + data[i] + "' invalid number format";
				}
				localNode.SetMemory(i, isBase26, d_content);
			} else {
				localNode.SetMemory(i, dataContent);
			}
			localNode.AddLabel(i, dataLabel);
		}

		success = true;
		return "Loading kit...";
	}*/
}