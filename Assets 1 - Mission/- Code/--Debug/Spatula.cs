using UnityEngine;
using System.Collections;

public class Spatula : MonoBehaviour {

	public Material material;
	public Texture2D texture;
	public Texture tex;

	void Start() {

		print( new XX.X( 9, 3 ) );

	}

}

public abstract class Base {

	public int i = 3;
	public int e = 3;

	public Base( int i, int e ) {
		this.i = i;
		this.e = e;
	}

	public override string ToString() { return i + "|" + e; }

}

public static class XX {

	public class X : Base {

		public X( int k, int j ) : base( k, k * j ) { }

	}

}