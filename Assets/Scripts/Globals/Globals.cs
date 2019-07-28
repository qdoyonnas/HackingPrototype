using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate string Command(string input, out bool success);
public delegate string CommandAction(NodeScript node, string input, int sourceInstruction, out bool success, out string commandName);
public delegate void CallbackAction(NodeScript node, string input, int sourceInstruction, bool success);
public class CommandDictionary : Dictionary<string, Command> {}

// Character Sets
public static class CharSets
{
	public const string NUMERIC = "0123456789";
	public const string ALPHANUMERIC = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string ALPHANUMERIC_UPPER = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string ALPHANUMERIC_LOWER = "0123456789abcdefghijklmnopqrstuvwxyz";
	public const string ALPHA = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string ALPHA_UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string ALPHA_LOWER = "abcdefghijklmnopqrstuvwxyz";
}

// Patterns
public static class P
{
	public const string DATA = @"[0-9a-zA-Z\:]+";
	public const string FLOAT = @"[+\-]?[.0-9a-zA-Z\:]+";
	public const string INT = @"[+\-]?[0-9a-zA-Z\:]+";
}

// Conversion
public static class AlphaNumeral
{
	public static double GetValueofChar( char c )
	{
		c = Char.ToLower(c);
		double value = CharSets.ALPHANUMERIC_LOWER.IndexOf(c);
		if( value < 0 ) {
			value = 0; // Any unrecognized symbol = 0
		}

		// Overlap 0-9 over A-J
		value -= 10;
		if( value < 0 ) {
			value += 10;
		}

		return value;
	} 

	public static bool IsStringBase26( string input )
	{
		if( input.IndexOfAny(CharSets.ALPHA.ToCharArray()) != -1 ) {
			return true;
		}

		return false;
	}

	public static bool StringToDbl( string input, out double result )
	{ 
		if( IsStringBase26(input) ) {
			return StringToDbl26(input, out result);
		} else {
			return StringToDbl10(input, out result);
		}
	}
	public static bool StringToInt( string input, out int result )
	{
		double val;
		bool returned = StringToDbl(input, out val);
		int i_val = (int)(val % int.MaxValue);

		result = i_val;
		return returned;
	}

	public static bool StringToDbl10( string input, out double result )
	{
		return double.TryParse(input, out result);
	}
	public static bool StringToInt10( string input, out int result )
	{
		double val;
		bool returned = StringToDbl10(input, out val);
		int i_val = (int)(val % int.MaxValue);

		result = i_val;
		return returned;
	}

	public static bool StringToDbl26( string input, out double result )
	{
		result = 0;
		input = input.ToLower();
		int targetBase = 26;
		int digits = input.Length - 1;
		int negative = 1;

		for( int i = digits; i >= 0; i-- ) {
			if( input[i] == '+' ) {
				digits -= 1;
				continue;
			}
			if ( input[i] == '-' ) {
				digits -= 1;
				negative = -1;
				continue;
			}

			double value = GetValueofChar(input[i]);

			value = (value * Math.Pow(targetBase, digits - i));
			result += value;
		}

		result *= negative;
		return true;
	}
	public static bool StringToInt26( string input, out int result )
	{
		double val;
		bool returned = StringToDbl26(input, out val);
		int i_val = (int)(val % int.MaxValue);

		result = i_val;
		return returned;
	}

	public static string DblToString( double input, string charSet = CharSets.ALPHA_UPPER )
	{

		int max = GameManager.gameOptions.dataLength;
		int i = max;
		char[] buffer = new char[max];
		int targetBase = charSet.Length;
		bool negative = input < 0;

		// Handle negative
		if( negative ) {
			input *= -1;
		}
		// wrap number within maximum value
		input = input % GameManager.gameOptions.alphaIntMaxValue;
		do {
			int idx = (int)(input % targetBase);
			buffer[--i] = charSet[idx];
			input = Math.Floor(input / targetBase);
		} while( input > 0 && i > 0 );

		if( input > 0 ) {
			Debug.LogError("DblToString failed: input > 0 and i <= 0");
			return "";
		}

		char[] result = new char[max - i];
		Array.Copy(buffer, i, result, 0, max - i);
		string s_result = new string(result);

		while( s_result.Length < GameManager.gameOptions.dataLength ) {
			s_result = charSet[0] + s_result;
		}
		s_result = ( negative ? "-" : "" ) + s_result;

		return s_result;
	}
	public static string IntToString( int input )
	{
		return DblToString((double)input, CharSets.ALPHA_UPPER);
	}

	public static double RandomDouble( double minValue, double maxValue )
	{
		double positive = Math.Floor(UnityEngine.Random.value * maxValue);
		double negative = Math.Floor(UnityEngine.Random.value * minValue);

		return positive + negative;
	}
	public static double RandomDouble()
	{
		return RandomDouble(double.MinValue, double.MaxValue);
	}
	public static string RandomString( double minValue, double maxValue, string charSet = CharSets.ALPHA_UPPER )
	{
		double value = RandomDouble(minValue, maxValue);
		return DblToString(value, charSet);
	}
}