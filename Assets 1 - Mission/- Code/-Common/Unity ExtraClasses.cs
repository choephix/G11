using UnityEngine;
using System.Collections;

public enum Side : byte { LEFT, CENTER, RIGHT }
public static class Sides {

	internal static Side FloatToSide( float n ) {

		if( n < 0 ) {
			return Side.LEFT;
		}

		if( n > 0 ) {
			return Side.RIGHT;
		}

		return Side.CENTER;

	}

}

public static class M {

	internal const float PI2 = Mathf.PI * 2.0f;

	internal static float ClipMaxMin( float theValue, float max = 1.0f, float min = 0.0f ) {
		if( theValue < min )
			return min;
		if( theValue > max )
			return max;
		return theValue;
	}

	internal static int ClipMaxMinInt( int theValue, int max, int min = 0 ) {
		if( theValue < min )
			return min;
		if( theValue > max )
			return max;
		return theValue;
	}

	internal static int LoopMaxMin( int theValue, int max, int min = 0 ) {
		if( theValue < min )
			return max;
		if( theValue > max )
			return min;
		return theValue;
	}

	internal static float Round( float value, int decimals ) {
		decimals *= 10;
		value *= decimals;
		value = Mathf.Round( value );
		value /= decimals;
		return value;
	}

	internal static int Round( float value ) {
		return (int)( value );
	}

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



	internal static bool IsIntInRange( int n, int max, int min = 0 ) {
		return ( n >= min && n <= max );
	}
	internal static bool IsInRange( float n, float max = 1f, float min = 0 ) {
		return ( n >= min && n <= max );
	}

}


public static class T {

	internal static float GetAngleTo( Transform from, Transform to ) {

		return ( from.eulerAngles.y - Vector3.Angle( from.position, to.position ) );

	}

}

public static class F {

	internal static void Break() {
		throw new UnityException();
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

public static class Verify {

	internal static bool NotNull( Object o, string name = "something" ) {
		if( o == null ) {
			Debug.LogWarning( name.ToUpper() + " IS NULL!" );
			return false;
		} else {
			return true;
		}
	}

	//internal static bool Type( Object o, System.Type type ) {
	//    if( o is type ) {
	//        Debug.LogWarning( name.ToUpper() + " IS NULL!" );
	//        return false;
	//    } else {
	//        return true;
	//    }
	//}

}
