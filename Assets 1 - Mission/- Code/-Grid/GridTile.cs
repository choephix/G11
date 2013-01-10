using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridTile : MissionBaseClass {

	public new Renderer renderer;

	public Transform cameraSpot;
	public GridTileAssets assets;

	public Color colorOk;
	public Color colorDanger;
	public Color colorCurrent;

	public TextMesh label;

	internal Unit currentUnit;
	internal Obstruction obstruction;
	internal Int2D location;

	private Material materialRegular;

	internal GridTileTileRelations relations;

	internal bool selectable = false;
	internal bool obstructed { get { return obstruction!=null; } }
	internal bool occupied { get { return currentUnit && currentUnit.alive; } }
	internal bool walkable { get { return !occupied && !obstructed; } }
	internal bool traversable { get { return walkable || (obstructed&&obstruction.height<=.5f); } }

	internal float coverValue { get { return cover==null?0:cover.coverValue; } }
	internal ICover cover {
		get {
			if( occupied ) return currentUnit;
			if( obstructed ) return obstruction;
			return null;
		}
	}

	internal bool fogged = true;
	internal int x { get { return location.x; } }
	internal int y { get { return location.y; } }

	void Start() {

		UpdateMaterial();
		label.renderer.enabled = false;
		relations = new GridTileTileRelations(this, grid.GetAllTiles());

		if( !Config.USE_FOG ) {
			UnFog();
		}

	}

	void Update() {
	//	renderer.enabled = selectable;
	}

	public void UpdateMaterial() {
		collider.enabled = true;
		if( occupied ) {
			if( currentUnit.selected ) {
				materialRegular = assets.materialOccupiedSelected;
			} else {
				materialRegular = assets.materialOccupied;
			}
		} else {
			if( selectable ) {
				materialRegular = assets.materialWalkable;
			} else {
				materialRegular = assets.materialHidden;
				collider.enabled = false;
			}
		}
		renderer.material = materialRegular;
		renderer.material.color = colorCurrent;
	}

	internal void Reset() {

		selectable = false;
		label.renderer.enabled = false;
		renderer.enabled = false;
		UpdateMaterial();

		if( focused ) {
			OnBlur();
		}

		transform.Find( "arrow" ).renderer.enabled = false;
		transform.Find( "flag" ).renderer.enabled = false;

	}

	internal void MakeSelectable() {

		selectable = true;
		renderer.enabled = true;

		float danger = 0;
		RangedAttackResult r;
		foreach( Unit u in allUnits ) {
			if( selectedUnit.team.IsEnemy( u ) ) {
				if( u.inPlay && u.CanSee( this ) ) {
					u.__SetFlag( true );
					r = new RangedAttackResult( u, selectedUnit, this );
					danger += r.hitChance / 100 * ( 1 - danger );
				}
			}
		}
		colorCurrent = Color.Lerp( colorOk, colorDanger, danger );

		UpdateMaterial();
		FadeIn();

	}

	/// <summary>
	/// MODIFICATIONS
	/// </summary>

	internal void setObstruction( Obstruction obstruction ) {
		this.obstruction = obstruction;
	}

	internal void clearObstruction() {
		this.obstruction = null;
	}

	/// <summary>
	/// MATH 'N STUFF
	/// </summary>
	


	/// <summary>
	/// EVENTS
	/// </summary>
	
	public bool focused;

	private void OnFocus() {

		focused = true;

		if( selectable && GameMode.Is( GameModes.PickTile ) ) {
			foreach( GridTile tile in relations.neighbours ) {
				if( tile.obstructed ) {
					tile.obstruction.holoUp();
				}
				//GodOfPathfinding.GetPathTo( this, true );
			}
			GodOfHolographics.HighlightGridTile( this );
		}

	//	OnFocusDebug();

	}

	private void OnBlur() {

		focused = false;

		foreach( GridTile tile in relations.neighbours ) {
			if( tile.obstructed ) {
				tile.obstruction.holoDown();
			}
		}

	//	Blink();
		GodOfHolographics.UnhighlightGridTile( this );

	//	OnBlurDebug();

	}

	internal void OnMouseExit(){
		OnBlur();
	}

	internal void OnMouseEnter() {
		OnFocus();
    }
	
	internal void OnMouseUp() {
		ClickHandler.Up(this);
	}

	override public string ToString() {
		return "[" + location.x + "x" + location.y + "]";
	}

	/// ViSUAL CRAP

	internal void FadeIn() {
		animation.Rewind();
		animation.Play( "show" );
		//	animation.PlayQueued( "IDLE" );
	}

	internal void Blink() {
		animation.Rewind();
		animation.Play( "blink" );
		//	animation.PlayQueued( "IDLE" );
	}

	internal void UnFog() {
		if( fogged ) {
			fogged = false;
			transform.Find( "fog" ).renderer.enabled = false;
			//transform.Find( "fog" ).animation.Play( "unfog" );
			if( currentUnit ) {
				currentUnit.model.meshRenderer.renderer.enabled = true;
			}
		}
	}

	/// DEVELOPMENT SHIT

	private void OnUnitSelectedDebug( Unit unit ) {
	}

	private void OnFocusDebug() {
		foreach( GridTile tile in relations.neighbours ) {

			float c = relations.GetRelation( tile ).diagonal ? .25f : .75f;
			float a = 1;
			a = 1 - ( Vector3.Distance( tile.transform.position, transform.position ) / 4 ) + .25f;

			tile.label.renderer.material.color = new Color( c, 1, c, a );
			tile.label.text = M.FixAngleDeg( relations.GetRelation( tile ).angle + 90 ).ToString();
			tile.label.renderer.enabled = true;

		}
		label.renderer.enabled = true;
		label.renderer.material.color = Color.white;
		label.text = M.Round( relations.GetTotalCoverValueAgainst( God.selectedUnit.currentTile ) * 100, 2 ).ToString() + "%";
	}

	private void OnBlurDebug() {
		foreach( GridTile tile in relations.neighbours ) {
			tile.label.renderer.enabled = false;
		}
		label.renderer.enabled = false;
	}

	public void DebugOut( string s, float alpha = 1.0f ) {
		label.renderer.enabled = true;
		label.renderer.material.color = new Color( 1, 1, 1, alpha );
		label.text = s;
		label.transform.localPosition = Vector3.up * ( coverValue * 2 + .2f );
	}

}

[System.Serializable]
public class GridTileAssets {
	public Material materialHidden;
	public Material materialWalkable;
	public Material materialWalkableOnMouseOver;
	public Material materialOccupied;
	public Material materialOccupiedSelected;
	public Material materialOccupiedOnMouseOver;
}

internal class GridTileTileRelations {

	internal const float COVER_MAX_ANGLE = 45;

	internal readonly Dictionary<GridTile,GridTileTileRelation> relations;
	internal GridTile[] neighbours;

	public GridTileTileRelations( GridTile @this, GridTile[,] allTiles ) {
		relations = new Dictionary<GridTile, GridTileTileRelation>();
		List<GridTile> covers = new List<GridTile>();
		GridTileTileRelation relation;
		foreach( GridTile @that in allTiles ) {
			relation = new GridTileTileRelation( @this, @that );
			relations.Add( @that, relation );
			if( @that != @this && relation.distanceSqr == 1 ) {
				covers.Add( @that );
			}
		}
		this.neighbours = covers.ToArray();
	}

	internal ICover[] GetCoversAgainst( GridTile attackerTile ) {

		List<ICover> covers = new List<ICover>();

		foreach( GridTile coverTile in neighbours ) {
			if( coverTile != attackerTile ) {
				if( GetSingleCoverValueAgainst( coverTile, attackerTile ) > 0 ) {
					covers.Add( coverTile.cover );
				}
			}
		}

		covers.Sort( ( ICover t1, ICover t2 ) => t2.coverValue.CompareTo( t1.coverValue ) );

		return covers.ToArray();

	}

	internal float GetTotalCoverValueAgainst( GridTile attackerTile ) {
		float result = 0;
		foreach( GridTile coverTile in neighbours ) {
			if( coverTile != attackerTile ) {
				result += GetSingleCoverValueAgainst( coverTile, attackerTile );
			}
		}
		return result;
	}

	internal float GetSingleCoverValueAgainst( GridTile coverTile, GridTile attackerTile ) {
		return
			( COVER_MAX_ANGLE - Mathf.Abs( Mathf.DeltaAngle(relations[coverTile].angle, relations[attackerTile].angle ) ).ClipMaxMin( COVER_MAX_ANGLE ) )
			/ COVER_MAX_ANGLE * coverTile.coverValue;
	}

	public GridTileTileRelation GetRelation( GridTile tile ) {
		return relations[tile];
	}
	public float GetAngle( GridTile tile ) {
		return relations[tile].angle;
	}
	public float GetDistance( GridTile tile ) {
		return relations[tile].distance;
	}

}

internal class GridTileTileRelation {
	 

	public readonly float angle;
	public readonly float distance;
	public readonly int distanceSqr;
	public readonly bool diagonal;

	//public readonly float walkDistance;
	//public readonly GridTile walkPrevious;

	public GridTileTileRelation( GridTile @this, GridTile @that ) {
		Vector2 thisXY = new Vector2( @this.transform.position.x, @this.transform.position.z );
		Vector2 thatXY = new Vector2( @that.transform.position.x, @that.transform.position.z );
		
		angle = M.FixAngleRad( Mathf.Atan2( @that.location.y - @this.location.y, @that.location.x - @this.location.x ) ) * Mathf.Rad2Deg;
		distance = Vector2.Distance( thisXY, thatXY );
		distanceSqr = GetDistanceSqr( @this.location, @that.location );
		diagonal = ( @that.location.y != @this.location.y ) && ( @that.location.x != @this.location.x );
	}

	private int GetDistanceSqr( Int2D a, Int2D b ) {
		return Mathf.Abs( a.x - b.x ) > Mathf.Abs( a.y - b.y ) ? Mathf.Abs( a.x - b.x ) : Mathf.Abs( a.y - b.y );
	}

}

internal static class GridTileMisc {

	internal static GridTile GetSmallestCoverTile( GridTile tile ) {
		return null;
	}

}