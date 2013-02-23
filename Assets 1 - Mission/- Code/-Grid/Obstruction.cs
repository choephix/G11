using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstruction : HoloObject, IDamageable, ICover {

	public float height = .5f; //TODO make x2 and covervalue /2
	public TextMesh label;
	public Material scratchesMaterial;

	public float coverValue { get { return height; } }

	internal GridTile currentTile;
	internal WorldObject decor;

	public Transform deathplosion;

	void Start () {
		HoloDown();
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

		//ScratchUp();

		Die( attacker );

	}

	public void Die( Unit attacker = null ) {

		decor.visible = false;
		Destroy( decor.gameObject );

		height = 0;

		Instantiate( deathplosion , transform.position , Quaternion.identity );

	}

	public void ScratchUp() {

		List<Material> list = new List<Material>();

		list.AddRange( decor.transform.Find("model").renderer.materials );

		Material m = new Material( scratchesMaterial ) {mainTextureOffset = new Vector2( rand , rand )};

		list.Add( m );

		decor.transform.Find( "model" ).renderer.materials = list.ToArray();

	}

	internal void HoloUp() {
		model.enabled = true;
		label.renderer.enabled = true;
	}
	internal void HoloDown() {
		label.renderer.enabled = false;
		model.enabled = false;
	}

}
