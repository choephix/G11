using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public partial class ProcessBook {


	public class UnitMoveToTile : UnitProcess {

		protected readonly GridTile targetTile;
		protected readonly float speed;

		protected float distanceTotal;
		protected float distanceCovered = 0f;
		protected float step;

		public UnitMoveToTile( Unit subject, GridTile targetTile, float speed = -1f )
			: base( "MoveToTile", subject ) {
			this.targetTile = targetTile;
			this.speed = speed > 0f ? speed : subject.movementSpeed;
		}

		protected override void _Start() {

			distanceTotal = subjectUnit.GetDistance( targetTile.transform );

			subjectUnit.transform.LookAt( targetTile.transform );

		}

		protected override void _Update() {

			step = GodOfTime.deltaTime * speed;
			subjectUnit.transform.position = Vector3.MoveTowards(
				subjectUnit.transform.position, targetTile.transform.position, step );
			distanceCovered += step;

			if( !( distanceCovered < distanceTotal ) ) {
				End();
			}

		}

		protected override void _End() {

			subjectUnit.currentTile = targetTile;
			subjectUnit.OnTileReached();

		}

	}

	


	public class UnitMoveAlongPath : UnitProcess {

		protected readonly List<GridTile> path;

		public UnitMoveAlongPath( Unit subject, GridTile targetTile )
			: base( "Repositioning", subject ) {
			path = GodOfPathfinding.GetPathTo( targetTile );
		}

		protected override void _Start() {

			Process p = new Nothing();

			processManager.AddImmediately( p );

			p = path.Aggregate( p , ( current , tile ) => current.Enqueue( new UnitMoveToTile( subjectUnit , tile ) ) );

			p.eventEnded += End;

			subjectUnit.model.animator.moving = true;

		}

		protected override void _End() {

			subjectUnit.model.animator.moving = false;

			subjectUnit.transform.position = subjectUnit.currentTile.transform.position;
			subjectUnit.OnCurrentTileReached();

		}

	}

}



