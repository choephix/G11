using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public static class Angles {

	internal const float PI2 = Mathf.PI * 2.0f;

	internal static float FixAngleDeg( float fi ) {
		while( fi >= 360f ) fi -= 360f;
		while( fi < 0.0f ) fi += 360f;
		return fi;
	}

	internal static float FixAngleDegSigned( float fi ) {
		while( fi > 180f ) fi -= 360f;
		while( fi <= -180f ) fi += 360f;
		return fi;
	}

	internal static float FixAngleRad( float fi ) {
		while( fi >= PI2 ) fi -= PI2;
		while( fi < 0.0f ) fi += PI2;
		return fi;
	}

}

public static class G {

	


}

public static class F {

	internal static void Break( string s = "" ) {
		throw new UnityException( s );
	}

	internal static void Spit( object s ) {
		Debug.LogWarning( s.ToString() );
	}

	internal static string ToStringOrNull( object o ) {
		return o != null ? o.ToString() : "NULL";
	}

	internal static void NullCheck( Object o, string name = "something" ) {
		if( o == null ) {
			Debug.LogWarning( name.ToUpper() + " IS NULL!" );
		}
	}

}