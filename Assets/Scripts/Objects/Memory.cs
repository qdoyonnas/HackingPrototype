using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MemoryCell
{
	public bool isBase26;
	public bool indirect;
	public double value;

	public string zero {
		get {
			return (isBase26 ? "A" : "0");
		}
	}
	
	private string _content;
	public string content {
		get {
			if( _content == null ) {
				_content = new string(zero[0], GameManager.gameOptions.dataLength);
			}

			return _content;
		}
		set {
			// Guarantee that there is a dataLength string in _content
			if( _content == null ) {
				_content = new string(zero[0], GameManager.gameOptions.dataLength);
			}

			// Format input value
			string formattedValue = value;
			// XXX: this needs to be smarter (not add values if a command)
			while( formattedValue.Length < GameManager.gameOptions.dataLength ) {
				formattedValue = zero + formattedValue;
			}

			// Iterate through strings keeping old chars where values are ==
			//	and using new chars where values are !=
			char[] merged = new char[GameManager.gameOptions.dataLength];
			for( int x=0; x < GameManager.gameOptions.dataLength; x++ ) {
				double oldVal = AlphaNumeral.GetValueofChar(_content[x]);
				double newVal = AlphaNumeral.GetValueofChar(formattedValue[x]);
				if( oldVal == newVal ) {
					merged[x] = _content[x];
				} else {
					merged[x] = formattedValue[x];
				}
			}

			_content = new string(merged);
		}
	}

	public void ExplicitSetContent(string content)
	{
		/*while(content.Length < GameManager.gameOptions.dataLength) {
			content = zero + content;
		}*/

		// Use with caution
		_content = content;
	}

	public override string ToString()
	{
		return (value < 0 ? "-" : "+") + content;
	}
}

public class MemoryBlock
{
	// Memory cells
	MemoryCell[] cells;

	public MemoryBlock()
	{
		cells = new MemoryCell[GameManager.gameOptions.memoryCellCount];
		for( int i=0; i < cells.Length; i++ ) {
			SetContent(i, AlphaNumeral.RandomDouble());
		}
	}

	public MemoryCell GetCell(int idx)
	{
		return cells[idx];
	}

	public string SetContent(int idx, double value)
	{
		if( idx < 0 || idx >= GameManager.gameOptions.memoryCellCount ) {
			return "subIndex out of bounds";
		}

		// Interpret
		cells[idx].value = value;
		if( value.ToString().Length <= GameManager.gameOptions.dataLength ) {
			cells[idx].isBase26 = false;
			cells[idx].content = value.ToString();
		} else {
			// Number too large = switch to base-26
			cells[idx].isBase26 = true;
			string converted = AlphaNumeral.DblToString(value);
			if( converted.Length > GameManager.gameOptions.dataLength ) {
				return "'" + converted + "' number larger than data size";
			}
			cells[idx].content = converted;
		}

		return null;
	}
	public string SetContent(int idx, string content)
	{
		if( idx < 0 || idx >= GameManager.gameOptions.memoryCellCount ) {
			return "subIndex out of bounds";
		}

		// XXX: The used symbols for modifiers should be stored somewhere else
		if( content[0] == '*' ) {
			cells[idx].indirect = true;
			content = content.Substring(1);
		} else {
			cells[idx].indirect = false;
		}

		// Set new value of cell
		double value;
		if( !AlphaNumeral.StringToDbl(content, out value) ) {
			return "'" + content + "' invalid number format";
		}

		// Set content
		if( content.Length <= GameManager.gameOptions.dataLength ) {
			// Content fits = explicit
			cells[idx].value = value;
			cells[idx].ExplicitSetContent(content);
		} else {
			// Content does not fit = convert
			return SetContent(idx, value);
		}

		return null;
	}

	public override string ToString()
	{
		string toString = "";

		toString += cells[0];
		for( int i=1; i < cells.Length; i++ ) {
			toString += "  " + cells[i];
		}

		return toString;

		/*
		// Format index
		string index = idx.ToString();
		int numOfDigits = Mathf.CeilToInt(Mathf.Log10(GameManager.gameOptions.nodeMemoryLength) );
		while( index.Length < numOfDigits ) {
			index = "0" + index;
		}

		// Format data
		string contents = memory.contents;
		if( memory.isData && memory.GetValue() >= 0 ) {
			contents = "+" + contents;
		}

		// Keep spacing consistent
		int TABLENGTH = 4;
		int tabCount = 3 - ((node.GetLabel(idx).Length+2) / TABLENGTH);

		return (showIndex ? index + "\t" : "") + "[" + node.GetLabel(idx).ToUpper() + "]" + 
			new string('\t', tabCount) + (node.IsMemoryQueued(idx) ? ">" : "") + "\t" + contents + "\n";
		*/
	}
}