using UnityEngine;
using System.Collections;

public static class Extensions {

	public static Vector3 Flatten( this Vector3 v ) {
		return new Vector3( v.x, 0, v.z );
	}

	public static float Round( this float v, int decimals ) {
		return M.Round( v, decimals );
	}

	internal static float ClipMaxMin( this float theValue, float max = 1.0f, float min = 0.0f ) {
		if( theValue < min )
			return min;
		if( theValue > max )
			return max;
		return theValue;
	}



}