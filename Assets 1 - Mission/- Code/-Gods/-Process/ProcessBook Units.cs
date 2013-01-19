using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public partial class ProcessBook : MissionBaseClass {






	// UNIT PROCESSES

	public abstract class UnitProcess : Process {

		protected readonly Unit subjectUnit;
		protected readonly bool evenInDeath;

		protected UnitProcess( string name, Unit subject, bool evenInDeath = false )
			: base( name ) {
			this.evenInDeath = evenInDeath;
			subjectUnit = subject;
		}

		public sealed override void Update() {

			if( subjectUnit.concious || evenInDeath ) {

				base.Update();

			} else {

				End();

			}

		}

	}

	public class HighlightTilesInVisibleRange : UnitProcess {

		protected readonly float range;

		public HighlightTilesInVisibleRange( Unit subject, float range )
			: base( "HighlightTilesInRange", subject ) {

			this.range = range;

		}

		protected override void _Start() {

			grid.ResetTiles();

			foreach( GridTile tile in grid.GetAllTiles() ) {

				if( subjectUnit.CanSee( tile ) && subjectUnit.GetDistance( tile ) < range ) {

					tile.MakeSelectable();

				}

			}

			End();

		}

	}

	public class HighlightWalkableTiles : UnitProcess { //TODO add abstract base class CalculateSelectables

		protected readonly float range;

		public HighlightWalkableTiles( Unit subject, float customRange = 0 )
			: base( "HighlightWalkableTiles", subject ) {

			range = customRange > 0 ? customRange : subject.propMovementRange;

		}

		protected override void _Start() {

			GodOfPathfinding.CalculateWalkables( subjectUnit, range );
			Events.calculateSelectablesFinished += End;

		}

		protected override void _End() {
			Events.calculateSelectablesFinished -= End;
		}

	}

	//public class HighlightWalkableTiles2 : UnitProcess {

	//	protected readonly Unit subjectUnit;
	//	protected readonly GridTile startTile;
	//	protected readonly float range;

	//	private readonly List<GridTile> prevStepNodes = new List<GridTile>();
	//	private readonly List<GridTile> nextStepNodes = new List<GridTile>();

	//	private readonly Dictionary<GridTile,PathNode> nodes = new Dictionary<GridTile, PathNode>();

	//	float tempPathLen;


	//	public HighlightWalkableTiles2( Unit subject, float customRange = 0 )
	//		: base( "HighlightWalkableTiles", subject ) {

	//		subjectUnit = subject;
	//		startTile = subject.currentTile;
	//		range = customRange > 0 ? customRange : subject.propMovementRange;

	//	}

	//	protected override void _Start() {

	//		grid.ResetTiles();

	//		nextStepNodes.Clear();
	//		prevStepNodes.Clear();
	//		prevStepNodes.Add( startTile );
	//		nodes.Add( startTile, new PathNode( null, 0 ) );

	//	}

	//	protected override void _Update() {


	//	}


	//}

	public class UnitMoveAlongPath : UnitProcess {

		public readonly Unit movingUnit;

		protected readonly GridTile targetTile;
		protected readonly List<GridTile> path;

		protected GridTile nextTile;
		protected float nextTileDistance;
		protected float stepLength;

		public UnitMoveAlongPath( Unit subject, GridTile targetTile )
			: base( "Reposition", subject ) {
			this.movingUnit = subject;
			this.targetTile = targetTile;
			this.path = GodOfPathfinding.GetPathTo( targetTile );
		}

		protected override void _Start() {

			subjectUnit.currentTile = targetTile; //TODO make every next tile currentTile

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
					processManager.Add( new BulletTime( .22f, .4f ), true );
				}

			}

		}

		protected override void _End() {

			subjectUnit.transform.position = subjectUnit.currentTile.transform.position;
			subjectUnit.OnCurrentTileReached();

		}

	}


	public class UnitAttack : UnitProcess {

		protected readonly Unit attacker;
		protected readonly Unit attackee;

		protected readonly AttackResult result;
		protected readonly Weapon weapon;

		public UnitAttack( Unit attacker, Unit attackee )
			: base( "Attack", attacker ) {

			this.attacker = attacker;
			this.attackee = attackee;

			result = attacker.relations.GetAttackResult( attackee );
			weapon = attacker.currentWeapon;

		}

		protected override void _Start() {

			Debug.Log( attacker + " attacks " + attackee + ". Hit chance: " + result.hitChance );

			if( attacker.canAttack ) {

				//GameMode.cinematic = true;

				bool successful = result.msg == AttackResult.Message.SUCCESS;

				attacker.model.Attack( attackee, successful );
				weapon.Attack( attackee, result.hittee );

				Process p;

				p = new WaitForSeconds( weapon.ranged ? UnitAnimation.RANGE_HIT_DELAY : UnitAnimation.MELEE_HIT_DELAY );

				//Attach( p );
				processManager.Add( p, true );

				if( result.hittee != null ) {
					p.eventEnded +=
						() => result.hittee.Damage( attacker.propAttackDamage, attacker.currentWeapon.damageType, attacker );
				}

				p.eventEnded += End;

				Logger.Respond( "Attack result: " + result );

			} else {

				Debug.Log( "Attack failed because " + attacker + " cannot attack. " );
				End();

			}

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}

	public class UnitHeal : UnitProcess {

		public const float HEALING_SPEED = .25f;

		public readonly float healingAmount;
		public float healingProgress;

		public UnitHeal( Unit subject, float amount )
			: base( "Healing", subject ) {
			healingAmount = amount;
		}

		protected override void _Update() {

			if( healingProgress < healingAmount ) {

				healingProgress += HEALING_SPEED;
				subjectUnit.propHealth += HEALING_SPEED;

			} else {

				End();

			}

		}

	}

	public class Throw : UnitProcess {

		protected const float SPEED		= .045f;
		protected const float HEIGHT	= 1.5f;

		protected readonly Unit thrower;
		protected readonly Transform throwee;

		protected Vector3 positionEnd;
		protected Vector3 positionStart;
		protected float distance;

		protected float progress;

		public Throw( Unit thrower, Transform throwee, Vector3 destination, float inaccuracy = 0f )
			: base( "Throw", thrower ) {

			this.thrower = thrower;
			this.throwee = throwee;

			positionEnd = destination;

			if( !inaccuracy.NotZero() ) return;

			positionEnd += Vector3.forward * Random.Range( -inaccuracy, inaccuracy );
			positionEnd += Vector3.left * Random.Range( -inaccuracy, inaccuracy );

		}

		protected override void _Start() {
			thrower.model.Reload();
			positionStart = throwee.transform.position;
			distance = Vector3.Distance( positionStart, positionEnd );
			throwee.transform.parent = null;
			throwee.transform.localPosition = Vector3.zero;
		}

		protected override void _Update() {

			progress += SPEED;

			throwee.position = CalculatePosition();

			if( progress >= 1f ) {
				End();
			}

		}

		private Vector3 CalculatePosition() {

			Vector3 r;

			r = Vector3.MoveTowards( positionStart, positionEnd, progress * distance );

			r += Vector3.up * Mathf.Sin( progress * Mathf.PI ) * HEIGHT;

			return r;

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}




















}
