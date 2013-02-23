using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public abstract class Action {

	protected Unit owner;
	protected readonly object source;

	public readonly string name;
	public readonly ActionSubjectType subjectType;
	public readonly byte cost;
	public bool oneUse;

	public readonly bool instant;
	public readonly byte cooldown;
	public int cooldownLeft;

	public bool visible { get { return owner.canAct && _visible; } }
	protected virtual bool _visible { get { return _possible; } } //TODO implemento
	public bool possible { get { return owner.canAct && cooldownLeft == 0 && owner.status.actionPoints >= cost && _possible; } }
	protected virtual bool _possible { get { return true; } }
	public bool canDoAgain { get { return possible && _canDoAgain; } }
	protected virtual bool _canDoAgain { get { return subjectType != ActionSubjectType.Self; } }

	public event EventHandler<Action> eventFinished = delegate { };


	protected internal Action(
				Unit owner, object source, string name = "Unnamed",
				ActionSubjectType subjectType = ActionSubjectType.Self,
				byte cooldown = 0, byte cost = 1, bool instant = false
			) {

		this.owner = owner;
		this.source = source;
		this.name = name;
		this.subjectType = subjectType;
		this.cooldown = cooldown;
		this.cost = cost;
		this.instant = instant;

	}

	public void SetOwner( Unit unit ) {
		owner = unit;
	}

	public bool IsSubjectViable( object subject ) { return _IsSubjectViable( subject ); }

	public virtual bool _IsSubjectViable( object subject ) { return true; }

	public void Execute( object subject ) {

		cooldownLeft = cooldown;

		owner.OnActionStart( this );

		_Execute( subject );

	}

	public abstract void _Execute( object subject );

	public void OnSelected() {

		Debug.Log( owner + " selected action " + ToLongString() );

		_OnSelected();

	}

	public virtual void _OnSelected() { }

	public void Finish() {

		owner.OnActionFinished( this );

		eventFinished.Invoke( this );

		//OnDeselected();

	}

	protected virtual void _Finish() { }

	public virtual void OnDeselected() { }

	public void OnTurnStart() {

		if( cooldownLeft > 0 ) {
			cooldownLeft--;
		}

		_OnTurnStart();

	}

	protected virtual void _OnTurnStart() { }



	public virtual string ToLongString() {
		return "{" + name + "}";
	}

	public override string ToString() {
		return possible ? '{' + name + '}' : '/' + name + '/';
	}

}

//public abstract class Action<T> : Action {

//    public virtual bool _IsSubjectViable( T subject ) { return true; }

//    public virtual bool _IsSubjectViable( object subject ) { return true; }

//    public virtual bool IsSubjectViable( object subject ) { return subject is T; }

//    protected internal Action(
//                Unit owner, object source, string name = "Unnamed",
//                ActionSubjectType subjectType = ActionSubjectType.Self,
//                byte cooldown = 0, byte cost = 1
//            )
//        : base( owner, source, name, subjectType, cooldown, cost, false ) { }

//    public override void _Execute() {
//        throw new UnityException( "don't use _Execute() for this type" );
//    }

//    public virtual void _Execute( object subject ) { }

//    public virtual void _Execute( T subject ) { }

//    public void Execute( T subject ) {

//        _Execute();

//        _Execute( subject );

//    }

//}

/// <summary>
/// 
/// GUIDELINES:
/// 
/// Override _possible and IsSubjectViable() to determine when the action can be used
/// 
/// Override _OnSelected() to set up the stage for picking a subjectUnit for your action.
/// (if it's an instant action (e.g. ActionSubjectType = Self) skip this)
/// 
/// Override _Execute() to queue processes and make other changes;
/// Call Finish() or assign it to an event to finish the action.
/// 
/// Override _Finish and OnDeselected to perform final tasks and clean-up if necessary.
/// 
/// </summary>
public class ActionsBook : MissionBaseClass {

	public abstract class InstantAction : Action {

		protected InstantAction( Unit owner, object source, string name, byte cooldown = 0, byte cost = 1 )
			: base( owner, source, name, ActionSubjectType.Self, cooldown, cost, true ) { }

		public override void _Execute( object subject ) {
			throw new UnityException( "InstantAction is abstract, fool." );
		}

	}

	public class Test : InstantAction {

		public Test( Unit owner )
			: base( owner, owner, "Test", 3 ) { }

		public override void _Execute( object subject ) {

			Process p = new ProcessBook.Wait( 10 );
			processManager.Add( p );

			//p = p.Enqueue( new ProcessBook.AreaDamage( owner.spots.torso.position, 4, 4 ) );

			Watcher w = new WatchingUnit<ProcessBook.UnitMoveAlongPath>( owner );
			processManager += w;
			w.instances = 2;
			w.eventWillStart +=
				delegate( Process process ) {

					Debug.LogWarning( ( (ProcessBook.UnitMoveAlongPath)process ).subjectUnit + " IS MOVING MOVING MOVING!!!!" );

					Process p2 = new ProcessBook.Trace( "I'LL SHOOT HIM!!!!" );
					Process p3 = new ProcessBook.UnitAttack( owner, ( (ProcessBook.UnitMoveAlongPath)process ).subjectUnit );
					p2.Enqueue( p3 );
					process.Enqueue( p2 );
					//GodOfProcesses.JumpAdd( p3 );

				};

			owner.buffs += BuffBook.Bleeding( owner );

			p = p.Enqueue( new ProcessBook.Wait( 10 ) );

			p.eventEnded += Finish;

		}

	}

	public class CoverAlly : Action {

		public CoverAlly( Unit owner )
			: base( owner, owner, "Cover Ally", ActionSubjectType.Self, 3 ) { }

		public override void _Execute( object subject ) {
			Finish();
		}

	}

	public class Crouch : InstantAction {

		public Crouch( Unit owner )
			: base( owner, owner, "Crouch" ) { }

		protected override bool _possible { get { return !owner.buffs.HasBuff( BuffBook.Ducked( owner ).name ); } }

		public override void _Execute( object subject ) {

			owner.buffs.Add( BuffBook.Ducked( owner ) );

			Process p;
			p = new ProcessBook.WaitForSeconds( .75f );
			processManager.Add( p );
			p.eventEnded += Finish;

		}

	}

	public class Move : Action {

		private const int DELAY = 2;

		public Move( Unit owner, object source )
			: base( owner, source, "Move", ActionSubjectType.GridTile ) { }

		protected override bool _possible { get { return owner.canMove; } }
		protected override bool _canDoAgain { get { return false; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is GridTile && ( subject as GridTile ).selectable;
		}

		public override void _OnSelected() {
			GameMode.Set( GameModes.PickTile );
			processManager.Add( new ProcessBook.HighlightWalkableTiles( owner, .1f ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.HoloUnit;
		}

		public override void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " moving from " + owner.currentTile + " to " + subject );

				Process p;

				p = new ProcessBook.UnitMoveAlongPath( owner, subject as GridTile );
			//	p = new ProcessBook.UnitMoveToTile( owner, subject as GridTile );
				processManager.Add( p );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		public override void OnDeselected() {

			grid.ResetTiles();
			GameMode.Reset();
			GodOfHolographics.mode = GodOfHolographics.HoloMode.None;

		}

	}

	public class Shoot : Action {

		public Shoot( Unit owner, Weapon source )
			: base( owner, source, "Shoot", ActionSubjectType.Damageable ) { }

		protected override bool _possible { get { return owner.canAttack && ( (Weapon)source ).canAttack; } }

		public override void _OnSelected() {
			SelectionManager.TargetUnit();
			owner.model.animator.aiming = true;
		}

		public override void _Execute( object subject ) {
			if( subject is IDamageable ) {

				Process p;

				p = new ProcessBook.UnitAttack( owner, subject as Unit );
				processManager.Add( p );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );
		}

		public override void OnDeselected() {

			GameMode.Reset();
			owner.model.animator.aiming = false;

		}

	}

	public class Attack : Action {

		public Attack( Unit owner, Weapon source )
			: base( owner, source, "Attack", ActionSubjectType.Unit ) { }

		protected override bool _possible { get { return owner.canAttack; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is Unit && owner.CanTarget( subject as Unit );
		}

		public override void _OnSelected() {
			//	GameMode.Set( GameModes.PickUnit );
			SelectionManager.TargetUnit();
			owner.model.animator.aiming = true;
		}

		public override void _Execute( object subject ) {
			if( subject is Unit ) {

				Process p;

				p = new ProcessBook.UnitAttack( owner, subject as Unit );
				processManager.Add( p );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );
		}

		public override void OnDeselected() {

			GameMode.Reset();
			owner.model.animator.aiming = false;

		}

	}

	public class Defend : InstantAction {

		private readonly Buff buff;

		public Defend( Unit owner, object source )
			: base( owner, source, "Alert" ) {
			buff = BuffBook.Alert();
		}

		protected override bool _possible { get { return !owner.buffs.HasDuplicates( buff ); } }

		public override void _Execute( object subject ) {

			owner.buffs.Add( buff );

			Finish();

		}

	}

	public class ClearBuff : InstantAction {

		private readonly string buffName;
		private readonly bool all; //TODO implement

		public ClearBuff( Unit owner, string actionName, string buffName, bool all = true )
			: base( owner, owner, actionName ) {
			this.buffName = buffName;
			this.all = all;
		}

		protected override bool _possible { get { return owner.buffs.HasBuff( buffName ); } } //TODO this should not be string

		public override void _Execute( object subject ) {

			owner.buffs.Remove( buffName );

			Finish();

		}

	}

	//----------------------------------------------------

	public class Reload : InstantAction {

		public Reload( Unit owner, Firearm source )
			: base( owner, source, "Reload" ) { }

		protected override bool _possible { get { return ( ( Firearm ) source ).ammoLeft < ( ( Firearm ) source ).ammoClipSize; } }

		public override void _Execute( object subject ) {

			owner.model.Reload();
			( ( Firearm ) source ).Reload();
			Logger.Respond( owner + " reloading." );

			Finish();

		}

	}

	public class Heal : InstantAction {

		protected readonly int healAmount;

		public Heal( Unit owner, Equippable source, int amount )
			: base( owner, source, "Heal " + amount ) { oneUse = true; healAmount = amount; }

		protected override bool _possible { get { return !owner.status.fullHealth; } }

		public override void _Execute( object subject ) {

			Process p = new ProcessBook.UnitHeal( owner, healAmount );
			processManager.Add( p );
			p.eventEnded += Finish;

		}

	}

	public abstract class ThrowAction : Action {

		protected ThrowAction( Unit owner, Throwable source, string name )
			: base( owner, source, name, ActionSubjectType.GridTile ) {
			oneUse = true;
		}

		public override bool _IsSubjectViable( object subject ) {
			return subject is GridTile && ( subject as GridTile ).selectable;
		}

		protected override bool _possible { get { return true; } }
		protected override bool _canDoAgain { get { return false; } }

		public override sealed void _OnSelected() {

			GameMode.Set( GameModes.PickTile );

			processManager.Add( new ProcessBook.HighlightTilesInVisibleRange( owner, ( ( Throwable ) source ).throwRange ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.Cross;
			GodOfHolographics.setRange( ( (Throwable)source ).effectRange );
			owner.model.Hide( owner.currentWeapon );
			owner.model.Show( source as Equippable );
			owner.model.Equip( source as Equippable, owner.model.mainHand );
			owner.model.Reload();

		}

		public override sealed void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " throwing " + source + " to " + subject );

				GridTile destinationTile = subject as GridTile;

				Vector3 destination = destinationTile.transform.position;

				Process p;

				p = new ProcessBook.Wait( 10 );
				processManager.Add( p );

				if( destinationTile.occupied && owner.team.IsAlly( destinationTile.currentUnit ) ) {

					destination += Vector3.up * destinationTile.currentUnit.propHeight;
					p = p.Enqueue( new ProcessBook.Throw( owner, source as Grenade, destination ) );
					p = p.Enqueue( new ProcessBook.Wait( 10 ) );
					//p = p.Enqueue( new ProcessBook.InstantProcess( () => PassEquipment( destinationTile.currentUnit ) ) );

				} else {

					if( destinationTile.obstructed ) {
						destination += Vector3.up * destinationTile.obstruction.height * 2;
					}
					p = p.Enqueue( new ProcessBook.Throw( owner, source as Grenade, destination, .5f ) );
					p = p.Enqueue( new ProcessBook.Wait( 10 ) );
					p = p.Enqueue( OnFallen( destinationTile ) );

				}

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		protected void PassEquipment( Unit passee ) {
			God.PassEquipment( owner, passee, (Equippable)source );
		}

		protected override sealed void _Finish() {

			owner.model.Equip( source as Equippable, owner.model.mainHand );

			owner.model.Hide( source as Equippable );
			owner.model.Show( owner.currentWeapon );
			owner.model.Reload();

			//OnDeselected();

		}

		public override sealed void OnDeselected() {

			grid.ResetTiles();
			GameMode.Reset();
			GodOfHolographics.mode = GodOfHolographics.HoloMode.None;
			owner.model.Hide( source as Equippable );
			owner.model.Show( owner.currentWeapon );

		}

		protected abstract Process OnFallen( GridTile destinationTile );

	}

	public class ThrowGrenade : ThrowAction {

		public ThrowGrenade( Unit owner, Grenade source )
			: base( owner, source, "ThrowGrenade" ) { oneUse = true; }

		protected override Process OnFallen( GridTile destinationTile ) {

			Grenade source = this.source as Grenade;

			if( source == null ) throw new UnityException("source=null?!");

			return new ProcessBook.AreaDamage(
				destinationTile.transform.position,
				source.effectRange,
				source.damage );

		}

	}

	public class Jump : Action {

		public Jump( Unit owner, Equippable source )
			: base( owner, source, "JumpTo", ActionSubjectType.GridTile, 2 ) { }

		protected override bool _possible { get { return owner.canMove; } }
		protected override bool _canDoAgain { get { return false; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is GridTile && ( subject as GridTile ).selectable;
		}

		public override void _OnSelected() {
			GameMode.Set( GameModes.PickTile );
			processManager.Add( new ProcessBook.HighlightTilesInVisibleRange( owner, owner.propMovementRange ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.HoloUnit;
		}

		public override void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " moving from " + owner.currentTile + " to " + subject );

				Process p = new ProcessBook.UnitJump( owner, subject as GridTile );
				processManager.Add( p );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		public override void OnDeselected() {

			grid.ResetTiles();
			GameMode.Reset();
			GodOfHolographics.mode = GodOfHolographics.HoloMode.None;

		}

	}

	public class Stomp : Action {

		public Stomp( Unit owner, Equippable source )
			: base( owner, source, "Stomp!", ActionSubjectType.GridTile ) { }

		protected override bool _possible { get { return owner.canMove; } }
		protected override bool _canDoAgain { get { return false; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is GridTile && ( subject as GridTile ).selectable && ( subject as GridTile ).walkable;
		}

		public override void _OnSelected() {
			GameMode.Set( GameModes.PickTile );
			processManager.Add( new ProcessBook.HighlightTilesInVisibleRange( owner, owner.propMovementRange ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.HoloUnit;
		}

		public override void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " stomping from " + owner.currentTile + " to " + subject );

				Process p = new ProcessBook.UnitJump( owner, subject as GridTile );
				processManager.Add( p );

				p = p.Enqueue( new ProcessBook.AreaDamage( ( ( GridTile ) subject ).transform.position, 8, 8, DamageType.CONCUSSIVE ) );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		public override void OnDeselected() {

			grid.ResetTiles();
			GameMode.Reset();
			GodOfHolographics.mode = GodOfHolographics.HoloMode.None;

		}

	}

	public class Lunge : Action {

		public Lunge( Unit owner, Equippable source )
			: base( owner, source, "LungeAttack!", ActionSubjectType.Unit ) { }

		protected override bool _possible { get { return owner.canMove; } }
		protected override bool _canDoAgain { get { return false; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is Unit && ( subject as Unit ).targetable;
		}

		public override void _OnSelected() {
			GameMode.Set( GameModes.PickUnit );
			processManager.Add( new ProcessBook.HighlightTargetableUnits( owner ) );
			//SelectionManager.TargetUnit();
			owner.model.animator.aiming = true;
		}

		public override void _Execute( object subject ) {

			if( subject is Unit ) {

				Debug.Log( owner + " stomping from " + owner.currentTile + " to " + subject );

				Process p = new ProcessBook.UnitJump( owner ,  
					( (Unit)subject ).currentTile.relations.GetClosestNeighbourTo( owner.currentTile ) ,
					2f, 5f );
				processManager.Add( p );

				p = p.Enqueue( new ProcessBook.UnitAttack( owner , ( Unit ) subject ) );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		public override void OnDeselected() {

			grid.ResetTiles();
			GameMode.Reset();
			GodOfHolographics.mode = GodOfHolographics.HoloMode.None;
			owner.model.animator.aiming = false;

			foreach( Unit unit in allUnits ) {
				SelectionManager.MarkTargetable( unit, false );
			}

		}

	}

	public class ToxicRush : InstantAction {

		public ToxicRush( Unit owner, Equippable source )
			: base( owner, source, "Toxic Rush", 1, 0 ) { }

		protected override bool _possible { get { return owner.status.actionPoints < Config.BASE_UNIT_ACTIONS; } }

		public override void _Execute( object subject ) {

			owner.status.ResetActionPoints();
			owner.buffs.Add( BuffBook.Intoxicated( owner ) );

		}

	}

}

