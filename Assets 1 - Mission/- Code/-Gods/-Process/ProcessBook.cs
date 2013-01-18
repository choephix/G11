using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ProcessBook : MissionBaseClass {

	// COMMON PROCESSES

	public class SimpleProcess : Process {

		public SimpleProcess( string name, bool stackable = true ) : base( name, stackable ) { }

		public sealed override void Update() { base.Update(); }

	}

	public class InstantProcess : SimpleProcess {

		public event EventHandler eventOnStart;

		public InstantProcess( EventHandler eventOnStart, string name = null ) : base( name??"InstantProcess" ) { this.eventOnStart = eventOnStart; }

		protected override void _Start() {

			eventOnStart.Invoke();

			End();

		}

	}

	public class Trace : InstantProcess {

		public string msg;

		public Trace( string msg ) : 
			base( () => Debug.Log( msg ) , "Trace" ) {}

	}









	public class Wait : SimpleProcess {

		protected int framesLeft;

		public Wait( int frames ) : base( "WaitForFrames" ) { framesLeft = frames; }

		protected override void _Update() {
			if( framesLeft <= 0 ) {
				End();
			} else {
				framesLeft--;
			}
		}

		public override string ToString() {

			return name + "/" + framesLeft;

		}

	}

	public class WaitSeconds : SimpleProcess {

		protected float timeLeft;

		public WaitSeconds( float seconds ) : base( "WaitForSeconds" ) { timeLeft = seconds; }

		protected override void _Update() {
			if( timeLeft <= 0f ) {
				End();
			} else {
				timeLeft -= GodOfTime.deltaTime;
			}
		}

		public override string ToString() {

			return name + "/" + timeLeft.Round( 1 );

		}

	}












	public class ChangeTimeSpeed : SimpleProcess {

		private readonly float newSpeed;
		private readonly float processDuration;

		private float stepPerSecond;
		private bool negative;
		private bool instant;

		public ChangeTimeSpeed( float newSpeed, float processDuration = 0f )
			: base( "ChangeTimeSpeed", false ) {

			this.newSpeed = newSpeed;
			this.processDuration = processDuration;

		}

		protected override void _Start() {

			if( processDuration > 0f ) {

				stepPerSecond = newSpeed - GodOfTime.speed;
				negative = stepPerSecond < 0;
				stepPerSecond /= processDuration;

			} else {

				instant = true;

			}

		}

		protected override void _Update() {

			if( instant ) {
				GodOfTime.speed = newSpeed;
				End();
			} else {

				if( Math.Abs( GodOfTime.speed - newSpeed ) < Mathf.Epsilon || negative == ( GodOfTime.speed < newSpeed ) ) {
					GodOfTime.speed = newSpeed;
					End();
				} else {
					GodOfTime.speed += Time.deltaTime * stepPerSecond;
				}
			}

		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name + " (" + GodOfTime.speed.Round( 2 ) + '/' + newSpeed + ')';

		}

	}

	public class BulletTime : SimpleProcess {

		private readonly bool instant;
		private readonly float tempSpeed;
		private readonly float duration;
		private readonly float fade;
		private float time;

		private float stepPerSecond;

		public BulletTime( float tempSpeed, float duration = 1f, float fade = 0f )
			: base( "BulletTime", false ) {

			this.tempSpeed = tempSpeed;
			this.duration = duration;
			this.fade = fade;
			time = 0;
			instant = fade <= 0;

		}

		protected override void _Start() {

			if( !instant ) {

				stepPerSecond = tempSpeed - GodOfTime.speed;
				stepPerSecond /= fade;

			}

		}

		protected override void _Update() {

			time += Time.deltaTime;

			if( instant ) {

				if( time < duration ) {
					GodOfTime.speed = tempSpeed;
				} else {
					End();
				}

			} else {

				if( time < fade ) {

					GodOfTime.speed += Time.deltaTime * stepPerSecond;

				} else if( time < duration - fade ) {

					GodOfTime.speed = tempSpeed;

				} else if( time < duration ) {

					GodOfTime.speed -= Time.deltaTime * stepPerSecond;

				} else {

					End();

				}

			}

		}

		protected override void _End() {
			GodOfTime.speed = 1f;
		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name + " (" + GodOfTime.speed.Round( 2 ) + '/' + tempSpeed + ')';

		}

	}















	// UNIT PROCESSES

	public abstract class UnitProcess : Process {

		protected readonly Unit u;
		protected readonly bool evenInDeath;

		protected UnitProcess( string name, Unit subject, bool evenInDeath = false )
			: base( name ) {
			this.evenInDeath = evenInDeath;
			u = subject;
		}

		public sealed override void Update() {

			if( u.concious || evenInDeath ) {

				base.Update();

			} else {

				End();

			}

		}

	}

	public class HighlightTilesInVisibleRange : UnitProcess {

		protected readonly Unit subjectUnit;
		protected readonly float range;

		public HighlightTilesInVisibleRange( Unit subject, float range )
			: base( "HighlightTilesInRange", subject ) {

			subjectUnit = subject;
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

		protected readonly Unit subjectUnit;
		protected readonly float range;

		public HighlightWalkableTiles( Unit subject, float customRange = 0 )
			: base( "HighlightWalkableTiles", subject ) {

			subjectUnit = subject;
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

			u.currentTile = targetTile; //TODO make every next tile currentTile

			//if( inPlay && currentPath.Count >= propMovementRange ) {
			//    CameraMode.Set( CameraMode.RUN );
			//}

		}

		protected override void _Update() {

			nextTile = path[0];
			nextTileDistance = Vector3.Distance( u.transform.position, nextTile.transform.position );
			stepLength = GodOfTime.deltaTime * u.movementSpeed;

			u.model.LoopClip( nextTile.obstructed ? UnitAnimation.JUMP : UnitAnimation.MOVE, .2f );

			if( nextTileDistance < stepLength ) {

				OnNextTileReached();

			} else {

				u.transform.LookAt( nextTile.transform.position );
				u.transform.position = Vector3.MoveTowards( u.transform.position, nextTile.transform.position, stepLength );

			}

		}

		private void OnNextTileReached() {

			u.ClearFog( nextTile ); //TODO move to Grid

			path.RemoveAt( 0 );

			if( path.Count == 0 ) {

				End();

			} else {

				if( path[0].obstructed ) {
					processQueue.Add( new BulletTime( .22f, .4f ) );
				}

			}

		}

		protected override void _End() {

			u.transform.position = u.currentTile.transform.position;
			u.OnCurrentTileReached();

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

				p = new WaitSeconds( weapon.ranged ? UnitAnimation.RANGE_HIT_DELAY : UnitAnimation.MELEE_HIT_DELAY );
				processQueue.Add( p, true );

				if( result.hittee != null ) {
					p.eventEnded +=
						() => result.hittee.Damage( attacker.propAttackDamage , attacker.currentWeapon.damageType , attacker );
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
				u.propHealth += HEALING_SPEED;

			} else {

				End();

			}

		}

	}

	public class Throw : UnitProcess {

		protected const float SPEED		= .02f;
		//protected const float speed		= .04f;
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

















	public class AreaDamage : SimpleProcess {

		private Vector3 center;
		private readonly float range;
		private readonly float damageAmount;
		private readonly DamageType damageType;

		public AreaDamage( Vector3 center, float range, float damageAmount, DamageType damageType = DamageType.NORMAL )
			: base( "AreaDamage" ) {

			this.center = center;
			this.range = range;
			this.damageAmount = damageAmount;
			this.damageType = damageType;

		}

		protected override void _Start() {

			foreach( Unit unit in allUnits ) {

				if( Vector3.Distance( center, unit.transform.position ) < range ) {
					unit.Damage( damageAmount, damageType );
					unit.transform.LookAt( center.Flatten() );
				}

			}

			TempObject splosion = Instantiate(
				BookOfEverything.me.gfx[0], center, Quaternion.identity ) as TempObject;
			if( splosion != null ) 
				splosion.eventDeath += End;

			Instantiate(
				BookOfEverything.me.gfx[1], center, Quaternion.identity );

		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name;

		}

	}










}
