using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProcessQueue : MissionBaseClass {

	protected readonly List<Process> background = new List<Process>();
	protected readonly List<Process> queue = new List<Process>();

	public bool runningQueue = false;
	public bool runningBackground = false;
	public bool running { get { return runningQueue && runningBackground; } }

	public bool empty { get { return emptyQueue && emptyBackground; } }
	public bool emptyQueue { get { return queue.Count.Equals( 0 ); } }
	public bool emptyBackground { get { return background.Count.Equals( 0 ); } }
	public Process currentProcess { get { return emptyQueue ? null : queue[0]; } }

	void Awake() {
		processQueue = this;
	}

	void Update() {

		if( !emptyQueue ) {
			UpdateProcess( currentProcess );
		} else {
			runningQueue = false;
		}

		if( !emptyBackground ) {
			for( int i = 0 ; i < background.Count ; i++ ) {
				UpdateProcess( background[i] );
			}
		} else {
			runningBackground = false;
		}

	}

	void UpdateProcess( Process process ) {

		runningQueue = true;

		if( !process.started ) {
			OnProcessWillStart( process );
		}

		process.Update();

		if( process.ended ) {
			OnProcessEnded( process );
		}

	}

	private void OnProcessWillStart( Process process ) {

		if( process.hasAttached ) {
			background.AddRange( process.attachedPassiveProcesses );
		}

		if( !process.stackable ) {
			int i = 0;
			while( i < background.Count ) {
				if( background[i] != process && background[i].name.Equals( process.name ) ) {
					Remove( background[i] );
				} else {
					i++;
				}
			}
		}

	}

	public void Add( Process process, bool inBackground = false ) {

		Debug.Log( "Adding new process " + process + " to the " + ( inBackground ? "background." : "queue" ) );

		if( inBackground ) {

			this.background.Add( process );

		} else {

			queue.Add( process );

		}

	}

	public void JumpAdd( Process process ) {
		queue.Insert( runningQueue ? 1 : 0, process );
	}

	public void Remove( Process process ) {
		background.Remove( process );
		queue.Remove( process );
	}

	public void OnProcessEnded( Process process ) {
		Remove( process );
	}

	public void OnCurrentProcessEnded() {
		queue.RemoveAt( 0 );
	}



	public void AddDelay( int frames ) { Add( new ProcessBook.Wait( frames ) ); }

	public void AddDelay( float seconds ) { Add( new ProcessBook.WaitSeconds( seconds ) ); }



	public string ToGuiStringBackground() {

		string pre = "Background Processes: ";

		if( emptyBackground ) {
			return pre + "[empty]";
		} else {
			string s = "\n";
			foreach( Process p in background ) {
				s = "\n" + p.ToString() + s;
			}
			return pre + s;
		}

	}

	public string ToGuiString() {

		string pre = "Process Queue: ";

		if( emptyQueue ) {
			return pre + "[empty]";
		} else {
			string s = "\n";
			foreach( Process p in queue ) {
				s = "\n" + p.ToString() + s;
			}
			return pre + s;
		}

	}

	public override string ToString() {
		if( emptyQueue ) {
			return "[empty]";
		} else {
			return "current process: " + currentProcess.ToString();
		}
	}

	public static ProcessQueue operator +( ProcessQueue pq, Process p ) { pq.Add( p ); return pq; }

	//-- -- -- -- -- -- -- -- -- -- -- TREE

	protected readonly List<Process> current = new List<Process>();
	public bool aempty { get { return current.Count == 0; } }
	public bool arunning = false;

	void aUpdate() {

		if( !aempty ) {
			arunning = true;
			for( int i = 0 ; i < current.Count ; i++ ) {
				UpdateProcess( current[i] );
			}
		} else {
			arunning = false;
		}

	}

}



public abstract class Process {

	public readonly string name;
	public readonly bool stackable;

	public bool started = false;
	public bool ended = false;
	public event EventHandler eventStarted = delegate { };
	public event EventHandler eventEnded = delegate { };

	public bool hasAttached { get { return attachedPassiveProcesses.Count > 0; } }
	public readonly List<Process> attachedPassiveProcesses = new List<Process>();

	public readonly bool strictlyBackground = false; //TODO replace the process queues with ProcessTree and deprecate this shit

	public Process( string name, bool stackable = true ) {
		this.name = name;
		this.stackable = stackable;
	}

	public virtual void Update() {

		if( !started ) {
			Start();
		}

		if( !ended ) {
			_Update();
		}

	}

	protected virtual void _Update() { }

	protected void Start() {

		started = true;
		eventStarted.Invoke();

		_Start();

	}

	protected virtual void _Start() { }

	protected void End() {

		_End();

		ended = true;
		eventEnded.Invoke();

	}

	protected virtual void _End() { }

	public Process AttachPassive( Process process ) {

		attachedPassiveProcesses.Add( process );

		return process;

	}

	public override string ToString() {

		return name;

	}

	//-- -- -- -- -- -- -- -- -- -- -- TREE

	public readonly List<Process> enqueued = new List<Process>();

	public Process Enqueue( Process process ) {
		enqueued.Add( process );
		return process;
	}

}



public class ProcessBook : MissionBaseClass {

	// COMMON PROCESSES

	public class SimpleProcess : Process {

		public SimpleProcess( string name, bool stackable = true ) : base( name, stackable ) { }

		public sealed override void Update() { base.Update(); }

	}

	public class InstantProcess : SimpleProcess {

		public event EventHandler eventOnStart;

		public InstantProcess( EventHandler eventOnStart ) : base( "InstantProcess" ) { this.eventOnStart = eventOnStart; }

		protected override void _Start() {

			eventOnStart.Invoke();

			End();

		}

	}

	public class Nothing : SimpleProcess {

		public Nothing() : base( "Nothing" ) { }

		protected override void _Update() { End(); }

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

		private float newSpeed;
		private float processDuration;

		private float stepPerSecond;
		private bool negative;
		private bool instant = false;

		public ChangeTimeSpeed( float newSpeed, float processDuration = 0f )
			: base( "ChangeTimeSpeed", false ) {

			this.newSpeed = newSpeed;
			this.processDuration = processDuration;

		}

		protected override void _Start() {

			if( processDuration > 0f ) {

				this.stepPerSecond = newSpeed - GodOfTime.speed;
				this.negative = stepPerSecond < 0;
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

				if( GodOfTime.speed == newSpeed || negative == ( GodOfTime.speed < newSpeed ) ) {
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
			this.time = 0;

			this.instant = fade <= 0;

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

		public UnitProcess( string name, Unit subject, bool evenInDeath = false )
			: base( name ) {
			this.evenInDeath = evenInDeath;
			this.u = subject;
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
		protected readonly GridTile centerTile;
		protected readonly float range;

		public HighlightTilesInVisibleRange( Unit subject, float range )
			: base( "HighlightTilesInRange", subject ) {

			this.subjectUnit = subject;
			this.centerTile = subject.currentTile;
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

	public class HighlightWalkableTiles : UnitProcess {

		protected readonly Unit subjectUnit;
		protected readonly GridTile startTile;
		protected readonly float range;

		public HighlightWalkableTiles( Unit subject, float customRange = 0 )
			: base( "HighlightWalkableTiles", subject ) {

			this.subjectUnit = subject;
			this.startTile = subject.currentTile;
			this.range = customRange > 0 ? customRange : subject.propMovementRange;

		}

		protected override void _Start() {

			GodOfPathfinding.CalculateWalkables( subjectUnit, range );
			GodOfPathfinding.eventWalkablesCalculated += End;

		}


	}

	public class HighlightWalkableTiles2 : UnitProcess {

		protected readonly Unit subjectUnit;
		protected readonly GridTile startTile;
		protected readonly float range;

		private readonly List<GridTile> prevStepNodes = new List<GridTile>();
		private readonly List<GridTile> nextStepNodes = new List<GridTile>();

		private readonly Dictionary<GridTile,PathNode> nodes = new Dictionary<GridTile, PathNode>();

		float tempPathLen;


		public HighlightWalkableTiles2( Unit subject, float customRange = 0 )
			: base( "HighlightWalkableTiles", subject ) {

			this.subjectUnit = subject;
			this.startTile = subject.currentTile;
			this.range = customRange > 0 ? customRange : subject.propMovementRange;

		}

		protected override void _Start() {

			God.grid.ResetTiles();

			nextStepNodes.Clear();
			prevStepNodes.Clear();
			prevStepNodes.Add( startTile );
			nodes.Add( startTile, new PathNode( null, 0 ) );

		}

		protected override void _Update() {


		}


	}

	public class UnitMoveAlongPath : UnitProcess {

		protected readonly GridTile targetTile;
		protected readonly List<GridTile> path;

		protected GridTile nextTile;
		protected float nextTileDistance;
		protected float stepLength;

		public UnitMoveAlongPath( Unit subject, GridTile targetTile )
			: base( "Reposition", subject ) {
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
					processQueue.Add( new ProcessBook.BulletTime( .22f, .4f ) );
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

			this.result = attacker.relations.GetAttackResult( attackee );
			this.weapon = attacker.currentWeapon;

		}

		protected override void _Start() {

			Debug.Log( attacker + " attacks " + attackee + ". Hit chance: " + result.hitChance );

			if( attacker.canAttack ) {

				//GameMode.cinematic = true;

				bool successful = result.msg == AttackResult.Message.SUCCESS;

				attacker.model.Attack( attackee, successful );
				weapon.Attack( attackee, result.hittee );

				Process p;

				p = new ProcessBook.WaitSeconds( weapon.ranged ? UnitAnimation.RANGE_HIT_DELAY : UnitAnimation.MELEE_HIT_DELAY );
				processQueue.Add( p, true );

				if( result.hittee != null ) {
					p.eventEnded += delegate {
						result.hittee.Damage( attacker.propAttackDamage, attacker.currentWeapon.damageType, attacker );
					};
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

		public readonly float healingSpeed = .25f;

		public readonly float healingAmount;
		public float healingProgress;

		public UnitHeal( Unit subject, float amount )
			: base( "Healing", subject ) {
			this.healingAmount = amount;
		}

		protected override void _Update() {

			if( healingProgress < healingAmount ) {

				healingProgress += healingSpeed;
				u.propHealth += healingSpeed;

			} else {

				End();

			}

		}

	}

	public class Throw : UnitProcess {

		protected const float speed		= .02f;
		//protected const float speed		= .04f;
		protected const float height	= 1.5f;

		protected readonly Unit thrower;
		protected readonly Transform throwee;

		protected Vector3 positionEnd;
		protected Vector3 positionStart;
		protected float distance;

		protected Vector3 startPosition;

		protected float progress = 0;

		public Throw( Unit thrower, Transform throwee, Vector3 destination, float inaccuracy = 0f )
			: base( "Throw", thrower ) {

			this.thrower = thrower;
			this.throwee = throwee;

			positionEnd = destination;

			if( !inaccuracy.NotZero( ) ) return;

			positionEnd += Vector3.forward * Random.Range( -inaccuracy, inaccuracy );
			positionEnd += Vector3.left * Random.Range( -inaccuracy, inaccuracy );

		}

		protected override void _Start() {
			thrower.model.Reload();
			this.positionStart = throwee.transform.position;
			this.distance = Vector3.Distance( positionStart, positionEnd );
			throwee.transform.parent = null;
			throwee.transform.localPosition = Vector3.zero;
		}

		protected override void _Update() {

			progress += speed;

			throwee.position = CalculatePosition();

			if( progress >= 1f ) {
				End();
			}

		}

		private Vector3 CalculatePosition() {

			Vector3 r = new Vector3();

			r = Vector3.MoveTowards( positionStart, positionEnd, progress * distance );

			r += Vector3.up * Mathf.Sin( progress * Mathf.PI ) * height;

			return r;

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}

















	public class AreaDamage : SimpleProcess {

		private Vector3 center;
		private float range;
		private float damageAmount;
		private DamageType damageType;

		public AreaDamage( Vector3 center, float range, float damageAmount, DamageType damageType = DamageType.NORMAL )
			: base( "AreaDamage" ) {

			this.center = center;
			this.range = range;
			this.damageAmount = damageAmount;
			this.damageType = damageType;

		}

		protected override void _Start() {

			foreach( Unit unit in God.allUnits ) {

				if( Vector3.Distance( center, unit.transform.position ) < range ) {
					unit.Damage( damageAmount, damageType );
					unit.transform.LookAt( center.Flatten() );
				}

			}

			TempObject splosion = Instantiate(
				BookOfEverything.me.gfx[0], center, Quaternion.identity ) as TempObject;
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
