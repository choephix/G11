using UnityEngine;
using System.Collections;

public class Obstruction : HoloObject {

	public float coverValue = .5f;
	public TextMesh label;

	public bool holoFlag;

	void Awake() {
	//	coverValue = (Random.Range( 1, 4 ) * .25f);
	}

	void Update() {
		transform.localScale = new Vector3( 1, coverValue*2, 1 );
		label.text = (int)( coverValue * 100 ) + "%";
	}

	void Start () {
		holoOut();
	}

	internal void holoUp() {
		if( !holoFlag ) {
			holoFlag =
			model.enabled = true;
			label.renderer.enabled = true;
			//animation.Play( "holoUp" );
			//animation.CrossFadeQueued( "idle", .05f );
			animation.Play( "idle" );
			animation.Rewind();
		}
	}
	internal void holoDown() {
		if( holoFlag ) {
			holoFlag =
			label.renderer.enabled = false;
			animation.Rewind();
			animation.CrossFade( "holoDown", .05f );
		}
	}
	internal void holoOut() {
		label.renderer.enabled = false;
		model.enabled = false;
	}

}
