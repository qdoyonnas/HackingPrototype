using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interpreter
{
	protected NodeScript localNode;
	protected CommandDictionary commands = new CommandDictionary();

	public Interpreter()
	{
		SetupCommands();
	}

	// Commands that can be run everywhere go here
	protected virtual void SetupCommands()
	{
		// Node Commands
		commands.Add("log", LogCommand);
		commands.Add("label", LabelCommand);
		commands.Add("write", WriteCommand);
		commands.Add("send", SendCommand);
		commands.Add("setport", SetPortCommand);
		commands.Add("exec", ExecCommand);

		// Math Commands
		commands.Add("add", AddCommand);
		commands.Add("sub", SubCommand);
		commands.Add("mul", MulCommand);
		commands.Add("div", DivCommand);
	}

    /*protected string EvalLabels( string input, out string result, NodeScript node, bool allowIndirect=true )
    {
        result = input;

        // Evaluate Labels
        Regex labelPattern = new Regex(@"\[(" + P.INDEX + @")\]");
        MatchCollection matches = labelPattern.Matches(input);
        foreach (Match label in matches)
        {
            // if '*' proceeds the label remove the '*' but leave the label as is
			// if indirection is not allowed this is ignored
			int exempt = input.IndexOf(label.Value) - 1;
			if (exempt >= 0 && input[exempt] == '*')
			{
				if( !allowIndirect ) {
					return "Indirection not allowed for this command";
				}
				input = input.Remove(exempt, 1);
				continue;
			}

            // Validate label
            int idx;
            string error = node.ParseMemoryIndex(label.Groups[1].Value, out idx);
            if (error != null)
            {
                return error;
            }

            // Substitute label
            int pos = input.IndexOf(label.Value);
            input = input.Remove(pos, label.Value.Length);
            input = input.Insert(pos, node.GetMemory(idx).contents);
        }

        result = input;
        return null;
    }
	protected string EvalLabels( string input, out string result, bool allowIndirect=true )
	{
		return EvalLabels( input, out result, localNode, allowIndirect );
	}*/

	// Node Commands
	protected abstract string LogCommand( string input, out bool success );
	protected abstract string LabelCommand( string input, out bool success );
	protected abstract string WriteCommand( string input, out bool success );
	protected abstract string SendCommand( string input, out bool success );
	protected abstract string SetPortCommand( string input, out bool success );
	protected abstract string ExecCommand( string input, out bool success );


    // Math Commands
    protected abstract string AddCommand( string input, out bool success );
	protected abstract string SubCommand( string input, out bool success );
	protected abstract string MulCommand( string input, out bool success );
	protected abstract string DivCommand( string input, out bool success );

	// ACTIONS
	protected string LogAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName )
	{
		commandName = "Log";

		success = true;
		return input;
	}

	protected string LabelAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Label";
		success = false;
		
		Regex pattern = new Regex(@"^(" + P.INT + @")\s+(" + P.DATA + @")");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'label " + input + "' invalid label format";
		}
		string s_index = match.Groups[1].Value;
		string label = match.Groups[2].Value.ToUpper();

		// Parse index
		int i_index;
		int subIndex;
		string error = node.ParseMemoryIndex(s_index, sourceInstruction, out i_index, out subIndex);
		if( error != null ) {
			return error;
		}

		if( i_index == node.portPointer ) {
			return "Cannot change label of port";
		}

		// Validate label
		if( label.Length == 0 ) {
			// Clear label
			node.AddLabel(i_index, "");

			success = true;
			return "";
		}

		if( CharSets.NUMERIC.Contains(label[0].ToString()) ) {
			return "'" + label + "' in 'label " + input + "' may not start with a number";
		}
		if( label.Length > GameManager.gameOptions.labelLength ) {
			return "'" + label + "' in 'label " + input + "' is longer than maximum label length";
		}
		for( int i=0; i < node.GetMemoryLength(); i++ ) {
			if( label.ToLower() == node.GetLabel(i).ToLower() ) {
				return "'" + label + "' already exists";
			}
		}

		// Add label
		node.AddLabel(i_index, label);

		success = true;
		return "";
	}

	protected string WriteAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Write";
		success = false;
		
		Regex pattern = new Regex(@"^("+P.INT+@")\s+(.*)");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'write " + input + "' invalid write format";
		}
		string index = match.Groups[1].Value;
		string content = match.Groups[2].Value;
		
		// Parse index
		int idx;
		int subIndex;
		string error = node.ParseMemoryIndex(index, sourceInstruction, out idx, out subIndex);
		if( error != null ) {
			return error;
		}

		// Split content and store one in every cell 
		string[] parts = content.Split(new[] {' '});
		if( parts.Length + subIndex >= GameManager.gameOptions.memoryCellCount ) {
			return "Content does not fit in memoryBlock starting at subindex " + subIndex;
		}
		for( int i=subIndex; i < parts.Length; i++ ) {
			error  = node.SetMemory(idx, i, parts[i]);
			if( error != null ) {
				return error;
			}
		}

		success = true;
		return "";
	}

	protected string SendAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Send";
		success = false;
		
		// Parse
		Regex pattern = new Regex(@"^("+ P.DATA + @")\s+(.+)");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'send " + input + "' invalid send format";
		}
		
		// Validate
		string address = match.Groups[1].Value;
		double d_address;
		if( !AlphaNumeral.StringToDbl(address, out d_address) )
		{
			return "'" + address + "' in '" + input + "' invalid number fromat";
		}

		string content = match.Groups[2].Value;
		
		// Create and Send Packet
		string error;
		NodeScript target = null;
		if( NodeManager.CheckValidAddress(d_address) ) {
			target = NodeManager.GetNode(d_address, true);
			error = PacketManager.CreatePacket(node, target, content);
		} else {
			error = PacketManager.CreatePacket(node, null, content);
		}

		if( error != null ) {
			return error;
		}

		success = true;
		return "";
	}

	protected string SetPortAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "SetPort";
		success = false;
		
		int i_index;
		int subIndex;
		string error = node.ParseMemoryIndex(input, sourceInstruction, out i_index, out subIndex);
		if( error != null ) {
			return error;
		}

		node.portPointer = i_index;
		success = true;
		return "";
	}

	protected string ExecAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Execute";
		success = false;

		int index;
		int subIndex;
		string error = node.ParseMemoryIndex(input, sourceInstruction, out index, out subIndex);
		if( error != null ) {
			return error;
		}

		node.instructionQueue.Enqueue( new EnqueuedInstruction(node, index) );

		success = true;
		return "Executing program...";
	}

	string MathAction( NodeScript node, string input, int sourceInstruction, out bool success, string opp )
	{
        success = false;

		// Parse
		Regex pattern = new Regex(@"^("+P.INT+@")\s+("+P.INT+@")");
		Match match = pattern.Match(input);
		if( !match.Success ) {
			return "'" + input + "' in 'add " + input + "' invalid addition format";
		}
		string integer = match.Groups[1].Value;
		string index = match.Groups[2].Value;

		// Validate
        double i_integer;
		if( !AlphaNumeral.StringToDbl(integer, out i_integer) ) {
			return "'" + integer + "' in 'add " + input + "' invalid number format";
		}
		
        int i_index;
		int subIndex;
		string error = node.ParseMemoryIndex(index, sourceInstruction, out i_index, out subIndex);
		if( error != null ) {
			return error;
		}

		// Validate memory contents (can't add to it if it isn't a number)
		MemoryCell memory = node.GetMemory(i_index).GetCell(subIndex);
		double i_content = memory.value;
		
        double result = 0;
        switch( opp ) {
            case "add":
                result = i_content + i_integer;
                break;
            case "sub":
                result = i_content - i_integer;
                break;
            case "mul":
                result = i_content * i_integer;
                break;
            case "div":
                result = i_content / i_integer;
                break;
            default:
                Debug.LogError("MathAction ran with invalid opperator");
                break;
        }
		// Store
		node.SetMemory(i_index, subIndex, result);

        success = true;
		return "";
	}

	protected string AddAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{  
		commandName = "Add";
        return MathAction(node, input, sourceInstruction, out success, "add");
	}

	protected string SubAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Subtract";
		return MathAction(node, input, sourceInstruction, out success, "sub");
	}

	protected string MulAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Multiply";
        return MathAction(node, input, sourceInstruction, out success, "mul");
    }

	protected string DivAction( NodeScript node, string input, int sourceInstruction, out bool success, out string commandName  )
	{
		commandName = "Divide";
        return MathAction(node, input, sourceInstruction, out success, "div");
    }
}