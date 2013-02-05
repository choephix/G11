using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class Extensions {

	private static float rand { get { return Random.value; } }







	/// OBJECT

	internal static int Rand( this object @this, int i ) {
		return (int)Mathf.Floor( rand * i );
	}
	internal static float Rand( this object @this, float f, bool signed = false ) {
		if( !signed ) {
			return rand*f;
		} else {
			return ( rand*f*2 - f );
		}
	}

	internal static bool Chance( this object @this, float percent ) {
		return rand * 100 < percent;
	}
	internal static bool Chance( this object @this, int percent ) {
		return rand * 100 < percent;
	}
	internal static bool Chance1( this object @this, float percent ) {
		return rand < percent;
	}

	/// INT

	internal static bool IsIntInRange( this int @this, int max, int min = 0 ) {
		return ( @this >= min && @this <= max );
	}

	internal static int ClipMaxMinInt( this int @this, int max, int min = 0 ) {
		if( @this < min )
			return min;
		if( @this > max )
			return max;
		return @this;
	}

	internal static int LoopMaxMin( this int @this, int max, int min = 0 ) {
		//TODO Fix this with delta value and whiles
		if( @this < min )
			return max;
		if( @this > max )
			return min;
		return @this;
	}

	/// FLOAT

	public static bool NotZero( this float value ) {
		return Math.Abs( value - 0.0f ) > Mathf.Epsilon;
	}

	internal static bool IsInRange( this float @this, float max = 1f, float min = 0 ) {
		return ( @this >= min && @this <= max ) || ( @this >= max && @this <= min );
	}

	public static int Round( this float value ) {
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

	//public static string RoundToString( this float value, int decimals ) {

	//	if( decimals == 0 ) {
	//		return Round( value ).ToString();
	//	}

	//	int suff0s = 0;
		
	//	decimals *= 10;
	//	value *= decimals;
	//	value = Mathf.Round( value );

	//	for( int i = 1 ; i <= decimals ; i++ ) {

	//		if( ( ( int ) value ) % ( 10 * i ) == 0 ) {

	//			suff0s++;

	//		}

	//	}

	//	value /= decimals;

	//	string s = value.ToString();

	//	for( int i = 0 ; i < suff0s ; i++ ) {

	//		s += "0";

	//	}

	//	return s;

	//}

	public static float ClipMaxMin( this float theValue, float max = 1.0f, float min = 0.0f ) {
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

	/// IENUMERABLE / LIST / T[]

	public static T GetRandom<T>( this List<T> @this ) {
		return @this[ (int)Mathf.Floor( rand*@this.Count ) ];
	}

	public static T GetRandom<T>( this T[] @this ) {
		return @this[(int)Mathf.Floor( rand * @this.Length )];
	}

	/// VECTOR3

	public static float DistanceTo( this Vector3 @this, Vector3 v ) {
		return Vector3.Distance( @this, v );
	}

	public static Vector3 Flatten( this Vector3 @this ) {
		return new Vector3( @this.x, 0, @this.z );
	}

	/// TRANSFORMS

	public static void AttachTo( this Transform @this, Transform to ) {
		@this.transform.parent = to;
		@this.transform.localPosition = Vector3.zero;
		@this.transform.localRotation = Quaternion.identity;
	}

	public static float AngleTo( this Transform from, Transform to ) {

		return ( from.eulerAngles.y - Vector3.Angle( from.position, to.position ) );

	}

}