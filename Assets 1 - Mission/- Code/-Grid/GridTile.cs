using System;
using System.Globalization;
using System.Linq;
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

	internal bool selectable;
	internal bool obstructed { get { return obstruction != null; } }
	internal bool occupied { get { return currentUnit && currentUnit.alive; } }
	//internal bool walkable { get { return !occupied && !obstructed; } }
	internal bool walkable { get { return !occupied; } }
	internal bool traversable { get { return walkable || ( obstructed && obstruction.height <= .5f ); } }

	internal float coverValue { get { return cover == null ? 0 : cover.coverValue; } }
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

	internal List<WorldObject> objects = new List<WorldObject>();

	void Start() {

		UpdateMaterial();
		label.renderer.enabled = false;
		relations = new GridTileTileRelations( this, grid.GetAllTiles() );

		Fog();
		if( !Config.USE_FOG ) {
			UnFog();
		}

		if( obstructed ) {
			transform.position += Vector3.up * obstruction.height * 2f;
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

		colorCurrent = Color.Lerp( colorOk, colorDanger, CalculateDanger( selectedUnit ) );

		UpdateMaterial();
		FadeIn();

	}

	internal float CalculateDanger( Unit unit ) {

		float danger = 0;
		AttackResult r;

		foreach( Unit u in allUnits.Where( u => unit.team.IsEnemy( u ) ).Where( u => u.inPlay && u.CanSee( this ) ) ) {

			if( u.currentWeapon.ranged )
				r = new RangedAttackResult( u , unit , this );
			else 
				r = new MeleeAttackResult( u, unit );

			danger += r.hitChance / 100 * ( 1 - danger );

		}

		return danger;

	}

	/// <summary>
	/// MODIFICATIONS
	/// </summary>

	internal void SetObstruction( Obstruction obstruction ) {
		this.obstruction = obstruction;
	}

	internal void ClearObstruction() {
		obstruction = null;
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

		if( !selectable ) return;
		//if( !selectable || !GameMode.Is( GameModes.PickTile ) ) return;

		foreach( GridTile tile in relations.neighbours.Where( tile => tile.obstructed ) ) {
			tile.obstruction.HoloUp();
		}

		GodOfHolographics.HighlightGridTile( this );

		//	OnFocusDebug();

	}

	private void OnBlur() {

		focused = false;

		foreach( GridTile tile in relations.neighbours ) {
			if( tile.obstructed ) {
				tile.obstruction.HoloDown();
			}
		}

		//	Blink();
		GodOfHolographics.UnhighlightGridTile( this );

		//	OnBlurDebug();

	}

	internal void OnMouseExit() {
		OnBlur();
	}

	internal void OnMouseEnter() {
		OnFocus();
	}

	internal void OnMouseUp() {
		ClickHandler.Up( this );
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

	internal void Fog() {
		if( fogged ) return;
		fogged = true;
		//transform.Find( "fog" ).renderer.enabled = true;
		if( currentUnit ) {
			currentUnit.model.visible = false;
		}
		foreach( WorldObject o in objects ) {
			o.alpha = 0f;
			print( 56 );
		}
	}

	internal void UnFog() {
		if( !fogged ) return;
		fogged = false;
		transform.Find( "fog" ).renderer.enabled = false;
		if( currentUnit ) {
			currentUnit.model.visible = true;
		}
		foreach( WorldObject o in objects ) {
			o.alpha = 1f;
		}
	}

	/// DEVELOPMENT SHIT

	private void OnUnitSelectedDebug( Unit unit ) {
		if( unit == null ) throw new ArgumentNullException( "unit" );
	}

	private void OnFocusDebug() {
		foreach( GridTile tile in relations.neighbours ) {

			float c = relations.GetRelation( tile ).diagonal ? .25f : .75f;
			float a;
			a = 1 - ( Vector3.Distance( tile.transform.position, transform.position ) / 4 ) + .25f;

			tile.label.renderer.material.color = new Color( c, 1, c, a );
			tile.label.text = Angles.FixAngleDeg( relations.GetRelation( tile ).angle + 90 ).ToString();
			tile.label.renderer.enabled = true;

		}
		label.renderer.enabled = true;
		label.renderer.material.color = Color.white;
		label.text = relations.GetTotalCoverValueAgainst( selectedUnit.currentTile ).ToPercent( 2 );
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

[Serializable]
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
		neighbours = covers.ToArray();
	}

	internal GridTile GetClosestNeighbourTo( GridTile subject ) {

		GridTile result = null;
		float dist = float.MaxValue;

		foreach( GridTile neighbour in neighbours ) {
			if( result != null && neighbour.transform.position.DistanceTo( subject.transform.position ) >= dist ) continue;
			result = neighbour;
			dist = neighbour.transform.position.DistanceTo( subject.transform.position );
		}

		return result;

	}

	internal ICover[] GetCoversAgainst( GridTile attackerTile ) {

		List<ICover> covers = ( from coverTile in neighbours where coverTile != attackerTile where GetSingleCoverValueAgainst( coverTile , attackerTile ) > 0 select coverTile.cover ).ToList( );

		covers.Sort( ( t1 , t2 ) => t2.coverValue.CompareTo( t1.coverValue ) );

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
			( COVER_MAX_ANGLE - Mathf.Abs( Mathf.DeltaAngle( relations[coverTile].angle, relations[attackerTile].angle ) ).ClipMaxMin( COVER_MAX_ANGLE ) )
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

		angle = Angles.FixAngleRad( Mathf.Atan2( @that.location.y - @this.location.y, @that.location.x - @this.location.x ) ) * Mathf.Rad2Deg;
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