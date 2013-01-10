using UnityEngine;
using System.Collections;

public class TileSelector : HoloObject {

	[SerializeField]
	private Renderer dangerBar;

	private Texture2D tex;

	// Use this for initialization
	void Start() {

		dangerBar.material.mainTexture = tex = new Texture2D( 32, 32 );

		ShowDanger( 0 );
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShowDanger( float percent ) {
		dangerBar.enabled = true;
		GUI_TextureFactory.PaintPercentBar( tex, percent );
		animation.Rewind();
		animation.Play( "show" );
	}
	public void HideDanger( ) {
		dangerBar.enabled = false;
	}

}
