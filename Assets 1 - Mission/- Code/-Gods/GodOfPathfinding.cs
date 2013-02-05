using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GodOfPathfinding : MissionBaseClass {

	internal static GodOfPathfinding me;

	internal static readonly Dictionary<GridTile,PathNode> nodes = new Dictionary<GridTile, PathNode>();

	void Start() {

		me = this;

	}

	public static List<GridTile> GetPathTo( GridTile tile ) {
		return me ? me._GetPathTo( tile ) : null;
	}

	protected List<GridTile> _GetPathTo( GridTile tile ) {

		List<GridTile> list = new List<GridTile>();

		while( nodes[tile].prevTile != null ) {

			list.Add( tile );
			tile = nodes[tile].prevTile;

		}

		list.Reverse();

		return list;

	}

	public static List<GridTile> GetLine( GridTile from, GridTile to ) {
		return me ? me._GetLine( from, to ) : null;
	}

	protected List<GridTile> _GetLine( GridTile from, GridTile to ) {

		List<GridTile> list = new List<GridTile>();
		IEnumerable<Int2D> r;
		GridTile t;

		r = GetPointsOnLine( from.x, from.y, to.x, to.y );
		foreach( Int2D value in r ) {
			t = grid.GetTile( value.x, value.y );
			if( t != from && t != to ) {
				list.Add( t );
			}
		}
		r = GetPointsOnLine( to.x, to.y, from.x, from.y );
		foreach( Int2D value in r ) {
			t = grid.GetTile( value.x, value.y );
			if( !list.Contains( t ) && t != from && t != to ) {
				list.Add( t );
			}
		}

		return list;

	}

}

internal class PathNode {

	public GridTile prevTile;

	public float pathLen;

	public PathNode( GridTile prevTile, float pathLen ) {
		this.prevTile = prevTile;
		this.pathLen = pathLen;
	}

}