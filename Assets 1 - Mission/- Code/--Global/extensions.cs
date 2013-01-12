using UnityEngine;
using System.Collections;

public static class Extensions {










	/// FLOAT

	internal static int Round( this float value ) {
		return (int)( value );
	}

	public static float Round( this float value, int decimals ) {

		if( decimals == 0 ) {
			return Round( value );
		}

		decimals *= 10;
		value *= decimals;
		value = Mathf.Round( value );
		value /= decimals;
		return value;

	}

	internal static float ClipMaxMin( this float theValue, float max = 1.0f, float min = 0.0f ) {
		if( theValue < min )
			return min;
		if( theValue > max )
			return max;
		return theValue;
	}




	public static float ToPercent( this float f ) {
		return f * 100;
	}

	public static string ToPercent( this float f, int decimals ) {
		return ( f*100 ).Round( decimals ) + "%";
	}

	/// VECTOR3

	public static float DistanceTo( this Vector3 @this, Vector3 v ) {
		return Vector3.Distance( @this, v );
	}

	public static Vector3 Flatten( this Vector3 @this ) {
		return new Vector3( @this.x, 0, @this.z );
	}

	/// TRANSFORMS

	public static float AngleTo( this Transform from, Transform to ) {

		return ( from.eulerAngles.y - Vector3.Angle( from.position, to.position ) );

	}

}