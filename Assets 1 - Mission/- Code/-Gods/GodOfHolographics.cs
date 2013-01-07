using UnityEngine;
using System.Collections;
using System.Collections.Generic; //TODO remove this later

public class GodOfHolographics : MissionBaseClass {

	public enum HoloMode { HoloUnit, Cross, None };

	public static GodOfHolographics me;

	public static HoloMode mode = HoloMode.None;

	public Transform gridTileHighlighter;
	public HoloObject unitHoloRunning;
	public HoloObject unitHoloInCover;
	public HoloObject unitHoloInCoverDucked;

	private static HoloObject unitHolo;

	public void Start() {
		me = this;
		hideAllHolos();
	}

	void Update() {
	}

	private void hideAllHolos() {
		unitHoloRunning.model.renderer.enabled = false;
		unitHoloInCover.model.renderer.enabled = false;
		unitHoloInCoverDucked.model.renderer.enabled = false;
		if( GameObject.Find( "MissionSet/Holo/Cross" ) != null ) {
			GameObject.Find( "MissionSet/Holo/Cross" ).GetComponent<HoloObject>().active = false;
		}
	}

	internal static void HighlightGridTile( GridTile gridTile ) {

		me.gridTileHighlighter.renderer.enabled = true;
		me.gridTileHighlighter.transform.position = gridTile.transform.position;

		me.hideAllHolos();

		if( mode == HoloMode.Cross ) {

			if( GameObject.Find( "MissionSet/Holo/Cross" ) != null ) {
				GameObject.Find( "MissionSet/Holo/Cross" ).GetComponent<HoloObject>().active = true;
				GameObject.Find( "MissionSet/Holo/Cross" ).transform.position = gridTile.transform.position;
			}

		} else if( mode == HoloMode.HoloUnit ) {

			if( gridTile.walkable ) {

				GridTile smallestCover = null;

				foreach( GridTile cover in gridTile.relations.neighbours ) {
					if( cover.obstructed && !gridTile.relations.relations[cover].diagonal ) {
						if( smallestCover == null || smallestCover.coverValue > cover.coverValue ) {
							smallestCover = cover;
						}
					}
				}

				unitHolo = me.unitHoloRunning;

				if( smallestCover == null ) {
					unitHolo = me.unitHoloRunning;
					unitHolo.model.renderer.enabled = true;
					unitHolo.transform.position = gridTile.transform.position;
					unitHolo.transform.LookAt( God.selectedUnit.transform );
					unitHolo.transform.Rotate( Vector3.up * 180 );
				} else {
					if( smallestCover.coverValue < God.selectedUnit.props.size ) {
						unitHolo = me.unitHoloInCoverDucked;
					} else {
						unitHolo = me.unitHoloInCover;
					}
					unitHolo.model.renderer.enabled = true;
					unitHolo.transform.position = gridTile.transform.position;
					unitHolo.transform.LookAt( smallestCover.transform );
				}

				unitHolo.animation.Rewind();
				unitHolo.animation.Play( "FadeIn" );

				unitHolo.transform.localScale = new Vector3( 1, God.selectedUnit.props.size, 1 );

				//gridTile.DebugOut( Mathf.Floor( M.FixAngleDegSigned( God.selectedUnit.currentTile.relations.GetRelation( gridTile ).angle + God.selectedUnit.transform.eulerAngles.y - 90 ) ).ToString() );


				//List<GridTile> list = GodOfPathfinding.GetLine( selectedUnit.currentTile, gridTile );
				//foreach( GridTile t in grid.GetAllTiles() ) {
				//    bool flag = list.Contains( t );
				//    t.transform.Find( "flag" ).renderer.enabled = flag;
				//}

			}

		}

	}

	internal static void UnhighlightGridTile( GridTile gridTile ) {

		me.gridTileHighlighter.renderer.enabled = false;
		me.hideAllHolos();

		if( GameObject.Find( "MissionSet/Holo/Cross" ) != null ) {
			GameObject.Find( "MissionSet/Holo/Cross" ).GetComponent<HoloObject>().active = false;
		}

		gridTile.Blink();

	}

}
