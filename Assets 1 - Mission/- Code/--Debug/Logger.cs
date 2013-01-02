using UnityEngine;
using System.Collections;

public class Logger : MissionBaseClass {

	public static string log { get { return _log; } }

	private static string _log = "";

	internal static void Error( string msg ) {
		print( msg );
	}

	internal static void Log( string msg ) {
		_log += msg + "\n";
	//	_log += ( StackTraceUtility.ExtractStackTrace() ) + "\n";
	}

	internal static void Respond( string msg ) {

		Log( msg );
		Debug.Log( msg );
		gui.Log( msg );
	
	}

	internal static void AlertGameDisabled() {
		print( "< < GAME DISABLED > >" );
	}

	internal static void Scream( int p ) {
		Scream( p.ToString() );
	}
	internal static void Scream( string p ) {
		Debug.LogWarning( "< < < < " + p.ToUpper() + " > > > >" );
	}

}