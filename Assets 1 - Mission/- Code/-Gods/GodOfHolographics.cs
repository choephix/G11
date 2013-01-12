using UnityEngine;
using System.Collections;
using System.Collections.Generic; //TODO remove this later

public class GodOfHolographics : MissionBaseClass {

	public enum HoloMode { HoloUnit, Cross, None };

	public static GodOfHolographics me;

	public static HoloMode mode = HoloMode.None;

	public TileSelector gridTileHighlighter;
	public HoloObject unitHoloRunning;
	public HoloObject unitHoloInCover;
	public HoloObject unitHoloInCoverDucked;

	private static HoloObject unitHolo;

	[SerializeField]
	private HoloObject _crossHolo;
	private static HoloObject crossHolo;

	public void Start() {
		me = this;
		crossHolo = _crossHolo;
		hideAllHolos();
	}

	void Update() {
	}

	private void hideAllHolos() {
		unitHoloRunning.model.renderer.enabled = false;
		unitHoloInCover.model.renderer.enabled = false;
		unitHoloInCoverDucked.model.renderer.enabled = false;
		if( crossHolo != null ) {
			crossHolo.active = false;
		}
	}

	internal static void HighlightGridTile( GridTile tile ) {

		me.gridTileHighlighter.renderer.enabled = true;
		me.gridTileHighlighter.transform.position = tile.transform.position + Vector3.up * .066f;

		me.hideAllHolos();

		if( mode == HoloMode.Cross ) {

			if( crossHolo != null ) {
				crossHolo.active = true;
				crossHolo.transform.position = tile.transform.position;
				if( tile.obstructed ) {
					crossHolo.transform.position += 
						Vector3.up * tile.obstruction.height * 2;
				}
			}

		} else if( mode == HoloMode.HoloUnit ) {

			if( tile.walkable ) {

				GridTile smallestCover = null;

				foreach( GridTile cover in tile.relations.neighbours ) {
					if( cover.obstructed && !tile.relations.relations[cover].diagonal ) {
						if( smallestCover == null || smallestCover.coverValue > cover.coverValue ) {
							smallestCover = cover;
						}
					}
				}

				unitHolo = me.unitHoloRunning;

				if( smallestCover == null ) {
					unitHolo = me.unitHoloRunning;
					unitHolo.model.renderer.enabled = true;
					unitHolo.transform.position = tile.transform.position;
					unitHolo.transform.LookAt( God.selectedUnit.transform );
					unitHolo.transform.Rotate( Vector3.up * 180 );
				} else {
					if( smallestCover.coverValue < God.selectedUnit.propHeight ) {
						unitHolo = me.unitHoloInCoverDucked;
					} else {
						unitHolo = me.unitHoloInCover;
					}
					unitHolo.model.renderer.enabled = true;
					unitHolo.transform.position = tile.transform.position;
					unitHolo.transform.LookAt( smallestCover.transform );
				}

				unitHolo.animation.Rewind();
				unitHolo.animation.Play( "FadeIn" );

				unitHolo.transform.localScale = new Vector3( 1, God.selectedUnit.props.size, 1 );

				//tile.DebugOut( Mathf.Floor( M.FixAngleDegSigned( God.selectedUnit.currentTile.relations.GetRelation( tile ).angle + God.selectedUnit.transform.eulerAngles.y - 90 ) ).ToString() );


				//List<GridTile> list = GodOfPathfinding.GetLine( selectedUnit.currentTile, tile );
				//foreach( GridTile t in grid.GetAllTiles() ) {
				//    bool flag = list.Contains( t );
				//    t.transform.Find( "flag" ).renderer.enabled = flag;
				//}
				
			}

		}

		//float danger = 0;
		//RangedAttackResult r;
		//foreach( Unit u in allUnits ) {
		//    if( selectedUnit.team.IsEnemy( u ) ) {
		//        if( u.inPlay && u.CanSee( tile ) ) {
		//            u.__SetFlag( true );
		//            r = new RangedAttackResult( u, selectedUnit, tile );
		//            danger += r.hitChance * (1-danger/100) ;
		//        }
		//    }
		//}
		//me.gridTileHighlighter.ShowDanger( danger );

	}

	internal static void UnhighlightGridTile( GridTile gridTile ) {

		me.gridTileHighlighter.renderer.enabled = false;
		me.hideAllHolos();

		me.gridTileHighlighter.HideDanger();

		if( crossHolo != null ) {
			crossHolo.active = false;
		}

		gridTile.Blink();

	}

	internal static void setRange( float p ) {
		//GameObject.Find( "MissionSet/Holo/Cross/range" ).transform.localScale = p * 2 * Vector3.one;
		crossHolo.renderers[2].transform.localScale = p * 2 * Vector3.one;
	}

}
