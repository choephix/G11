

using UnityEngine;

[System.Serializable]
public class Int2D { //These are classes because structs aren't visible in inspector (i think)

	public int x;
	public int y;

	public Int2D( int x, int y ) {
		this.x = x;
		this.y = y;
	}

	public override string ToString() {
		return "[" + x + "," + y + "]";
	}

}

[System.Serializable]
public class Int3D {

	public int x;
	public int y;
	public int z;

	public Int3D( int x, int y, int z ) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override string ToString() {
		return "[" + x + "," + y + "," + z + "]";
	}

}

public class ArgumentedBool {

	public string reason = "";

	public ArgumentedBool( string reason ) {
		this.reason = reason;
	}

	public static implicit operator bool( ArgumentedBool instance ) {
		return instance.reason == "";
	}

	public static implicit operator ArgumentedBool( string reason ) {
		return new ArgumentedBool( reason );
	}
	public static implicit operator ArgumentedBool( bool result ) {
		return new ArgumentedBool( result ? "" : "no reason given" );
	}

	public static ArgumentedBool Sample( int n ) {
		if( n > 0 )
			return "N(" + n + ") is higher than zero";
		if( n < 0 )
			return "N(" + n + ") is lower than zero";
		return true;
	}

}

// ReSharper disable InconsistentNaming
public class angle {
// ReSharper restore InconsistentNaming

	internal const float PI = Mathf.PI;
	internal const float PI2 = PI * 2.0f;

	private readonly float value;

	public angle( float value ) {
		this.value = Fix(value);
	}

	public float ToUnsigned() {
		if( value >= 0.0f ) {
			return value;
		}
		return value + PI2;
	}

	public float ToDeg() {
		return value * 180 / PI;
	}

	public override string ToString() {
		return ToDeg().ToString();
	}

	///STATICS

	internal static float Fix( float fi ) {
		while( fi >= PI ) fi -= PI2;
		while( fi < -PI ) fi += PI2;
		return fi;
	}

	internal static float FixUnsigned( float fi ) {
		while( fi >= PI2 ) fi -= PI2;
		while( fi < 0.0f ) fi += PI2;
		return fi;
	}

	/// OPERATORS

	public static implicit operator string( angle @this ) {
		return @this.ToString();
	}

	public static implicit operator float( angle @this ) {
		return @this.value;
	}

	public static implicit operator angle( float value ) {
		return new angle( value );
	}

	public static angle operator +( angle a, angle b ) {
		return new angle( a.value + b.value );
	}

}