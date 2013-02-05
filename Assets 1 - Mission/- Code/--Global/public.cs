using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class GodOfProcesses {

	public string ToGuiStringWatchers() {

		const string pre = "Process Watchers: ";

		if( watchers.Count == 0 ) {
			return pre + "[empty]";
		}

		string s = watchers.Aggregate( "\n", ( c, w ) => "\n" + w.ToString() + c );
		return pre + s;

	}

	public string ToGuiStringBackground() {

		return "<<DEPRECATED>>";

	}

	public string ToGuiString() {

		if( empty ) {
			return "[process pool empty]";
		}

		string s;
		s = activePool.Aggregate( "\n", ( current1, p ) => 
			"\n" + p.ToString(true) + current1 );
		s = "ActivePool: " + s;

		if( waitingList.Count > 0 ) {
			s += "\n\nWaiting List: ";
			s = waitingList.Aggregate( s , ( current , p ) => current + ( "\n" + p.ToString( true ) ) );
		}

		return s;

	}

	public override string ToString() {

		if( empty ) {
			return "[empty]";
		}

		return "activePool: " + activePool.Count;

	}

}










public static class GUI_TextureFactory {

	public static void PaintPercentBar( Texture2D tex, float percentage ) {

		float lim = percentage / 100 * tex.height;

		int i;
		int j;
		Color c;
		float a;

		int w = tex.width;
		int h = tex.height;

		for( j = 0 ; j < h ; j++ ) {
			for( i = 0 ; i < w ; i++ ) {
				//a = h - j > lim ? .125f : 1f;
				a = j > lim ? .125f : 1f;
				if( a >= 1f ) {
					a = j % 2 > 0 ? .75f : 1f;
				}
				if( j == 0 || j == ( h - 1 ) || i == 0 || i == ( w - 1 ) ) {
					a = .5f;
				}
				//a *= percentage / 100;
				c = percentage >= 50 ? 
					new Color( 1f, .2f, 0f, a ) : 
					new Color( 0f, .4f, 1f, a );
				tex.SetPixel( i, j, c );
			}
		}

		tex.Apply();

	}

}

















