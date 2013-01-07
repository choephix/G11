using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseClass : MonoBehaviour {

	internal delegate void Delegate<T>(T o);

	internal static void trace( string message, bool condition ) {
		if( condition ) {
			print( message );
		}
	}
	
	// SHIT WITH NUMBERS

    internal static float rand { get{ return Random.value;  } }
	
	internal static int Rand(int i)  { return (int)(rand*i); }
	internal static float Randomize(float f)  { return (rand*f-f/2); }
	internal static Vector2 Randomize(Vector2 v) { return new Vector2( rand*v.x, rand*v.y)-v/2; }
	internal static Vector3 Randomize(Vector3 v) { return new Vector3( rand*v.x, rand*v.y, rand*v.z)-v/2; }
	internal static Vector2 Loopalize(Vector2 v, float min , float max )   {
		float delta = max-min;
		while(v.x>max) v.x-=delta;
		while(v.y>max) v.y-=delta;
		while(v.x<min) v.x+=delta;
		while(v.y<min) v.y+=delta;
		return v;
	}

	internal static bool Chance( float v ) { return rand * 100 < v; }
	internal static bool Chance( int v ) { return rand * 100 < v; }
	internal static bool Chance1( float v ) { return rand < v; }

	//internal static void OffsetVector( ref Vector3 v, float x, float y = 0, float z = 0 ) {
	//    v = v + new Vector3( x, y, z );
	//}

	internal static void Foo() { print( "Bar" ); }

	public static IEnumerable<Int2D> GetPointsOnLine( int x0, int y0, int x1, int y1 ) {
		bool steep = Mathf.Abs( y1 - y0 ) > Mathf.Abs( x1 - x0 );
		if( steep ) {
			int t;
			t = x0; // swap x0 and y0
			x0 = y0;
			y0 = t;
			t = x1; // swap x1 and y1
			x1 = y1;
			y1 = t;
		}
		if( x0 > x1 ) {
			int t;
			t = x0; // swap x0 and x1
			x0 = x1;
			x1 = t;
			t = y0; // swap y0 and y1
			y0 = y1;
			y1 = t;
		}
		int dx = x1 - x0;
		int dy = Mathf.Abs( y1 - y0 );
		int error = dx / 2;
		int ystep = ( y0 < y1 ) ? 1 : -1;
		int y = y0;
		for( int x = x0 ; x <= x1 ; x++ ) {
			yield return new Int2D( ( steep ? y : x ), ( steep ? x : y ) );
			error = error - dy;
			if( error < 0 ) {
				y += ystep;
				error += dx;
			}
		}
		yield break;
	}

	public static void Spit( object s ) {
		if( s == null )
			Debug.LogWarning( "NULL" );
		else
			Debug.LogWarning( s.ToString() );
	}

}
