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

		owner.OnAction( this );

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

			Process p;


			p = new ProcessBook.Wait( 3 );
			processQueue += p;

			p = new ProcessBook.AreaDamage( owner.spots.torso.position, 4, 4 );
			processQueue += p;

			owner.buffs += BuffsBook.Bleeding( owner );

			p = new ProcessBook.Wait( 10 );
			processQueue += p;

			p.eventEnded += Finish;

		}

	}

	public class Watch : Action {

		public Watch( Unit owner )
			: base( owner, owner, "Test", ActionSubjectType.Self, 3 ) { }

		public override void _Execute( object subject ) {

			Process p;


			p = new ProcessBook.Wait( 10 );
			processQueue += p;

			p.eventEnded += Finish;

		}

	}

	public class Crouch : InstantAction {

		public Crouch( Unit owner )
			: base( owner, owner, "Crouch" ) { }

		protected override bool _possible { get { return !owner.buffs.HasBuff( BuffsBook.Ducked( owner ).name ); } }

		public override void _Execute( object subject ) {

			owner.buffs.Add( BuffsBook.Ducked( owner ) );

			Process p;
			p = new ProcessBook.WaitSeconds( .75f );
			processQueue += p;
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
			processQueue.Add( new ProcessBook.Wait( 1 ) );
			processQueue.Add( new ProcessBook.HighlightWalkableTiles( owner ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.HoloUnit;
		}

		public override void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " moving from " + owner.currentTile + " to " + subject );

				Process p;

				//p = new ProcessBook.ChangeTimeSpeed( .2f, 3f );
				//processQueue.Add( p, true );

				p = new ProcessBook.Wait( DELAY );
				processQueue.Add( p );

				p = new ProcessBook.UnitMoveAlongPath( owner, subject as GridTile );
				processQueue.Add( p );

				p = new ProcessBook.Wait( DELAY );
				processQueue.Add( p );
				p.AttachPassive( new ProcessBook.ChangeTimeSpeed( 1f, .5f ) );

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
		}

		public override void _Execute( object subject ) {
			if( subject is Unit ) {

				Process p;

				p = new ProcessBook.UnitAttack( owner, subject as Unit );
				processQueue += p;

				p = new ProcessBook.WaitSeconds( .20f );
				processQueue += p;

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );
		}

		public override void OnDeselected() {

			//else {
			//    if( !owner.canAttack ) {
			//        Debug.Log( owner + " cannot attack anymore." );
			//        GameMode.Reset();
			//    } else {
			//        if( targetedUnit != null && !targetedUnit.inPlay ) {
			//            Debug.Log( "Targeted unit no longer in play. Will switch to another." );
			//            SelectionManager.TargetAnotherUnit();
			//        }
			//        GameMode.Reenable();
			//    }


			//    else {
			//        GameMode.Reenable();
			//    }
			//}

			GameMode.Reset();

		}

	}

	public class Defend : InstantAction {

		private readonly Buff buff;

		public Defend( Unit owner, object source )
			: base( owner, source, "Alert" ) {
			buff = BuffsBook.Alert();
		}

		protected override bool _possible { get { return !owner.buffs.HasDuplicates( buff ); } }

		public override void _Execute( object subject ) {

			owner.buffs.Add( buff );

			Finish();

		}

	}

	public class StitchUp : InstantAction {

		public StitchUp( Unit owner )
			: base( owner, owner, "Stitch Up" ) { }

		protected override bool _possible { get { return owner.buffs.HasBuff( "Bleeding" ); } } //TODO this should not be string

		public override void _Execute( object subject ) {

			owner.buffs.Remove( "Bleeding" );

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
			processQueue += p;
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
			processQueue.Add( new ProcessBook.Wait( 1 ) );
			processQueue.Add( new ProcessBook.HighlightTilesInVisibleRange( owner, ( source as Throwable ).throwRange ) );
			GodOfHolographics.mode = GodOfHolographics.HoloMode.Cross;
			GodOfHolographics.setRange( ( (Throwable)source ).effectRange );
			owner.model.Hide( owner.currentWeapon );
			owner.model.Show( source as Equippable );
			owner.model.Equip( source as Equippable, owner.model.finger );
			owner.model.Reload();

		}

		public override sealed void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " throwing " + source + " to " + subject );

				GridTile destinationTile = subject as GridTile;

				Vector3 destination = destinationTile.transform.position;

				Process p;

				p = new ProcessBook.Wait( 30 );
				processQueue.Add( p );

				if( destinationTile.occupied && owner.team.IsAlly( destinationTile.currentUnit ) ) {

					destination += Vector3.up * destinationTile.currentUnit.propHeight;

					p = new ProcessBook.Throw( owner, source as Grenade, destination );
					processQueue.Add( p );

					p = new ProcessBook.Wait( 10 );
					processQueue.Add( p );

					//p = new ProcessBook.InstantProcess( () => PassEquipment( destinationTile.currentUnit ) );
					//processQueue.Add( p );

				} else {

					if( destinationTile.obstructed ) {
						destination += Vector3.up * destinationTile.obstruction.height * 2;
					}

					p = new ProcessBook.Throw( owner, source as Grenade, destination, .5f );
					processQueue.Add( p );

					p = new ProcessBook.Wait( 10 );
					processQueue.Add( p );

					p = OnFallen( destinationTile );
					processQueue.Add( p );

				}

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		protected void PassEquipment( Unit passee ) {
			God.PassEquipment( owner, passee, (Equippable)source );
		}

		protected override sealed void _Finish() {

			owner.model.Equip( source as Equippable, owner.model.finger );

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

}

