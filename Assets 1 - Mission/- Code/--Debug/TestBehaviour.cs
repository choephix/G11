using UnityEngine;
using System.Collections;

public class TestBehaviour : BaseClass {

	private Texture2D tex;

	void Start() {

		renderer.material.mainTexture = tex = new Texture2D( 32, 32 );

		GUI_TextureFactory.PaintPercentBar( tex, 0 );

	}




	public void OnMouseUp() {
		GUI_TextureFactory.PaintPercentBar( tex, rand * 100 );
	}

}

public static class GUI_TextureFactory {

	public static void PaintPercentBar( Texture2D tex, float percentage ) {

		float lim = percentage / 100 * tex.height;

		int i = 0;
		int j = 0;
		Color c;
		float a;

		int w = tex.width;
		int h = tex.height;

		for( j = 0 ; j < h ; j++ ) {
			for( i = 0 ; i < w ; i++ ) {
				//a = h - j > lim ? .125f : 1f;
				a = j > lim ? .125f : 1f;
				if( a == 1f ) {
					a = j % 2 > 0 ? .75f : 1f;
				}
				if( j == 0 || j == ( h - 1 ) || i == 0 || i == ( w - 1 ) ) {
					a = .5f;
				}
				//a *= percentage / 100;
				if( percentage >= 50 ) {
					c = new Color( 1f, .2f, 0f, a );
				} else {
					c = new Color( 0f, .4f, 1f, a );
				}
				tex.SetPixel( i, j, c );
			}
		}

		tex.Apply();

	}

}