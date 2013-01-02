using UnityEngine;
using System.Collections;

public class GridTileAnimated : MonoBehaviour {
	
	private GridTile me;
	//private Renderer model;

	public float maxPeriod = 10f;

	private float nextTick = 0;

	void Start() {

		me = GetComponent<GridTile>();
	//	model = me.renderer;

	}

	void Update () {

		if( me.selectable && nextTick < GodOfTime.time ) {

			Tick();

			nextTick = GodOfTime.time + ( Random.value * maxPeriod );

		}

	}

	private void Tick() {

		//model.transform.localPosition = Vector3.up * Random.value * maxHeight;
		animation.Play( "tick" );

	}

}
