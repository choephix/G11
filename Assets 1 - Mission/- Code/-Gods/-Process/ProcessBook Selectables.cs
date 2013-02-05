using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ProcessBook {

	public class HighLightTiles : Process {

		protected readonly GridTile[] subjects;

		public HighLightTiles( GridTile[] subjects )
			: base( "HighLightTiles" ) {

			this.subjects = subjects;

		}

		protected override void _Start() {

			foreach( GridTile tile in subjects ) {
				tile.MakeSelectable();
			}

		}

	}

	public class HighlightTilesInVisibleRange : UnitProcess {

		protected readonly float range;

		public HighlightTilesInVisibleRange( Unit subject , float range )
			: base( "HighlightTilesInRange" , subject ) {

			this.range = range;

		}

		protected override void _Start() {

			grid.ResetTiles();

			Process p = new Nothing();

			foreach( GridTile tile in grid.GetAllTiles() )
				if( subjectUnit.CanSee( tile ) && subjectUnit.GetDistance( tile ) < range )
					tile.MakeSelectable();

			End();

		}

	}

	public class HighlightWalkableTiles : UnitProcess {

		protected readonly GridTile startTile;
		protected readonly float range;

		private readonly List<GridTile> prevStepNodes = new List<GridTile>();
		private readonly List<GridTile> nextStepNodes = new List<GridTile>();

		//private readonly Dictionary<GridTile,PathNode> nodes = new Dictionary<GridTile, PathNode>();
		private readonly Dictionary<GridTile,PathNode> nodes = GodOfPathfinding.nodes;

		float tempPathLen;

		private readonly float stepPeriod;

		public HighlightWalkableTiles( Unit subject, float duration, float customRange = 0 )
			: base( "HighlightWalkableTiles", subject ) {

			startTile = subject.currentTile;
			range = customRange > 0 ? customRange : subject.propMovementRange;
			stepPeriod = duration / range;

		}

		protected override void _Start() {

			grid.ResetTiles();
			nodes.Clear();

			nextStepNodes.Clear();
			prevStepNodes.Clear();
			prevStepNodes.Add( startTile );
			nodes.Add( startTile, new PathNode( null, 0 ) );

			NextStep();

		}

		protected void NextStep() {

			prevStepNodes.Sort(
				( t1, t2 ) => nodes[t1].pathLen.CompareTo( nodes[t2].pathLen ) );

			foreach( GridTile prevTile in prevStepNodes.Where( prevTile => prevTile.traversable || prevTile == startTile ) ) {

				foreach( GridTile neighbour in prevTile.relations.neighbours ) {

					if( nodes.ContainsKey( neighbour ) && nodes[neighbour].prevTile != null ) {
						if( nodes[nodes[neighbour].prevTile].pathLen > nodes[prevTile].pathLen ) {
							nodes[neighbour].prevTile = prevTile;
							nodes[neighbour].pathLen = nodes[prevTile].pathLen + prevTile.relations.GetDistance( neighbour );
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

							}

						}

					}

				}

			}

			prevStepNodes.Clear();
			prevStepNodes.AddRange( nextStepNodes );
			nextStepNodes.Clear();

			if( prevStepNodes.Count > 0 ) {
				Process p = new Wait( 0 );
				processManager.AddImmediately( p );
				p.eventEnded += NextStep;
			} else {
				End();
			}

		}

	}


/*
	public class UnitMoveAlongPathDEPRECATING : UnitProcess {

		protected readonly GridTile targetTile;
		protected readonly List<GridTile> path;

		protected GridTile nextTile;
		protected float nextTileDistance;
		protected float stepLength;

		public UnitMoveAlongPathDEPRECATING( Unit subject, GridTile targetTile )
			: base( "Reposition", subject ) {
			this.targetTile = targetTile;
			this.path = GodOfPathfinding.GetPathTo( targetTile );
		}

		protected override void _Start() {

			//if( inPlay && currentPath.Count >= propMovementRange ) {
			//    CameraMode.Set( CameraMode.RUN );
			//}

		}

		protected override void _Update() {

			nextTile = path[0];
			nextTileDistance = Vector3.Distance( subjectUnit.transform.position, nextTile.transform.position );
			stepLength = GodOfTime.deltaTime * subjectUnit.movementSpeed;

			subjectUnit.model.LoopClip( nextTile.obstructed ? UnitAnimation.JUMP : UnitAnimation.MOVE, .2f );

			if( nextTileDistance < stepLength ) {

				OnNextTileReached();

			} else {

				subjectUnit.transform.LookAt( nextTile.transform.position );
				subjectUnit.transform.position = Vector3.MoveTowards( subjectUnit.transform.position, nextTile.transform.position, stepLength );

			}

		}

		private void OnNextTileReached() {

			subjectUnit.ClearFog( nextTile ); //TODO move to Grid

			path.RemoveAt( 0 );

			if( path.Count == 0 ) {

				End();

			} else {

				if( path[0].obstructed ) {
					processManager.AddImmediately( new BulletTime( .22f, .4f ) );
				}

			}

		}

		protected override void _End() {

			subjectUnit.transform.position = subjectUnit.currentTile.transform.position;
			subjectUnit.OnCurrentTileReached();

		}

	}
*/

}



