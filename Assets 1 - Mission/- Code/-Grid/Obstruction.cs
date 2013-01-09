using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstruction : HoloObject, IDamageable, ICover {

	public float height = .5f; //TODO make x2 and covervalue /2
	public TextMesh label;
	public Material scratchesMaterial;

	public bool holoFlag;

	public GridTile currentTile;
	public float coverValue { get { return height; } }

	public Transform decor;

	void Start () {
		holoOut();
	}

	void Update() {
		transform.localScale = new Vector3( 1, height * 2, 1 );
		label.text = (int)( coverValue * 100 ) + "%";
	}

	public void Damage( float amount, DamageType type, Unit attacker = null ) {

		//if( currentTile ) {
		//    currentTile.obstruction = null;
		//}

		//decor.gameObject.SetActiveRecursively( false );
		//GameObject.Destroy( decor.gameObject );

		//GodOfTheStage.me.AddDecor( GodOfTheStage.me.genericFloorTile, currentTile );

		//GameObject.Destroy( this );

		ScratchUp();

	}

	public void ScratchUp() {

		List<Material> list = new List<Material>();

		list.AddRange( decor.Find("model").renderer.materials );

		Material m = new Material( scratchesMaterial );
		m.mainTextureOffset = new Vector2( God.rand, God.rand );

		list.Add( m );

		decor.Find( "model" ).renderer.materials = list.ToArray();

	}

	internal void holoUp() {
		if( !holoFlag ) {
			holoFlag =
			model.enabled = true;
			label.renderer.enabled = true;
			//animation.Play( "holoUp" );
			//animation.CrossFadeQueued( "IDLE", .05f );
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
