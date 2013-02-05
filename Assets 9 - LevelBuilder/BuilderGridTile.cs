using UnityEngine;
using System.Collections;

public class BuilderGridTile : MonoBehaviour {

	public delegate void BuilderGridTileEvent( BuilderGridTile tile );
	public delegate void BuilderGridTileEvent<T>( BuilderGridTile tile, T param );
	public event BuilderGridTileEvent<int> onClick;
	public event BuilderGridTileEvent onOver;
	public event BuilderGridTileEvent onOut;

	internal Transform marker;
	private TextMesh label;

	public int x;
	public int y;

	public int floorType;

	public Transform currentObject;
	public Transform currentFloor;

	void Awake () {

		marker = transform.Find( "marker" );
		label = transform.Find( "label" ).GetComponent<TextMesh>();

	}

	void Start() {

		OnMouseExit();

	}

	internal void Init( int x, int y ) {

		this.x = x;
		this.y = y;
		transform.position = new Vector3( x , 0 , y );

	}

	void OnMouseEnter() {
		marker.renderer.enabled = true;
		onOver.Invoke( this );
	}

	void OnMouseExit() {
		marker.renderer.enabled = false;
		label.renderer.enabled = false;
		onOut.Invoke( this );
	}

	void OnMouseOver() {
		if( Input.GetMouseButtonDown( 0 ) ) {
			onClick.Invoke( this, 0 );
		}
		if( Input.GetMouseButtonDown( 1 ) ) {
			onClick.Invoke( this, 1 );
		}
		if( Input.GetMouseButtonDown( 2 ) ) {
			onClick.Invoke( this, 2 );
		}
	}

}
