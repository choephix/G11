
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
		return instance.reason=="";
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