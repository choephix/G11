using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GodOfPathfinding : MissionBaseClass {

	internal static GodOfPathfinding me;

	internal static bool ready = true;

	private readonly List<GridTile> prevStepNodes = new List<GridTile>();
	private readonly List<GridTile> nextStepNodes = new List<GridTile>();

	private readonly Dictionary<GridTile,PathNode> nodes = new Dictionary<GridTile,PathNode>();

	void Start() {

		me = this;

	}

	internal static void CalculateWalkables( Unit unit, float range ) {

		me.StartCoroutine( me.CalculateWalkablesGradually( unit.currentTile, range ) );

	}

	internal IEnumerator CalculateWalkablesGradually( GridTile startTile, float range ) {

		ready = false;

		grid.ResetTiles();
		nodes.Clear();

		nextStepNodes.Clear();
		prevStepNodes.Clear();
		prevStepNodes.Add( startTile );
		nodes.Add( startTile, new PathNode( null, 0 ) );

		float tempPathLen;
		
		byte i = 1 ;

		while( prevStepNodes.Count > 0 && i < 99 ) {

			i++;
			if( i == 20 ) { Debug.LogWarning( "TOO MUCH PATH" ); }
			yield return new WaitForSeconds( .02f ); // .04f

			prevStepNodes.Sort( 
				( t1 , t2 ) => nodes[t1].pathLen.CompareTo( nodes[t2].pathLen ) );

			foreach( GridTile prevTile in prevStepNodes ) {

				if( prevTile.traversable || prevTile==startTile ) {

					foreach( GridTile neighbour in prevTile.relations.neighbours ) {

						if( nodes.ContainsKey( neighbour ) && nodes[neighbour].prevTile != null ) {
							if( nodes[nodes[neighbour].prevTile].pathLen > nodes[prevTile].pathLen ) {
								nodes[neighbour].prevTile = prevTile;
								nodes[neighbour].pathLen = nodes[prevTile].pathLen + prevTile.relations.GetDistance( neighbour );
								if( Config.SHOW_PATH_ARROWS ) {
									neighbour.transform.Find( "arrow" ).transform.LookAt( prevTile.transform );
								}
							}
						} else {

							if( neighbour.traversable ) {

								tempPathLen = nodes[prevTile].pathLen;
								if( neighbour.obstructed ) {
									tempPathLen += prevTile.relations.GetDistance( neighbour ) + neighbour.coverValue * 2;
								} else {
									tempPathLen += prevTile.relations.GetDistance( neighbour );
								}

								if( tempPathLen < range ) {

									if( neighbour.walkable ) {
										neighbour.MakeSelectable();
									}

									nextStepNodes.Add( neighbour );
									nodes.Add( neighbour, new PathNode( prevTile, tempPathLen ) );

									//neighbour.DebugOut( Angles.Round( tempPathLen, 1 ).ToString() );

									if( Config.SHOW_PATH_ARROWS ) {
										neighbour.transform.Find( "arrow" ).renderer.enabled = true;
										neighbour.transform.Find( "arrow" ).transform.localPosition = Vector3.up * 0.03f;
										neighbour.transform.Find( "arrow" ).transform.LookAt( prevTile.transform );
										if( neighbour.obstructed ) {
											neighbour.transform.Find( "arrow" ).transform.localPosition =
												Vector3.up * ( neighbour.obstruction.coverValue * 2 + .1f );
										}
									}

								}


							}

						}

					}

				}

			}

			prevStepNodes.Clear();
			prevStepNodes.AddRange( nextStepNodes );
			nextStepNodes.Clear();

		}

		ready = true;
		Events.calculateSelectablesFinished.Invoke();

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

[System.Serializable]
public class PathNode {

	public GridTile prevTile;

	public float pathLen;

	public PathNode( GridTile prevTile, float pathLen ) {
		this.prevTile = prevTile;
		this.pathLen = pathLen;
	}

}