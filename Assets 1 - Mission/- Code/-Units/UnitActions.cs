using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class Action {

	protected readonly Unit owner;
	protected readonly object source;

	public readonly string name;
	public readonly ActionSubjectType subjectType;
	public readonly byte cost;

	public readonly bool instant;
	public readonly byte cooldown;
	public int cooldownLeft = 0;

	public bool visible { get { return owner.canAct && _visible; } }
	protected virtual bool _visible { get { return _possible; } } //TODO implemento
	public bool possible { get { return owner.canAct && cooldownLeft == 0 && owner.status.actionPoints >= cost && _possible; } }
	protected virtual bool _possible { get { return true; } }
	public virtual bool canDoAgain { get { return possible && subjectType != ActionSubjectType.Self; } }

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

		eventFinished.Invoke(this);

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
		return "{" + name + "<-" + source.ToString() + "}";
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

		public InstantAction( Unit owner, object source, string name, byte cooldown = 0, byte cost = 1 )
			: base( owner, source, name, ActionSubjectType.Self, cooldown, cost, true ) { }

		public override void _Execute( object subject ) {
			throw new UnityException( "InstantAction is abstract, fool." );
		}

	}

	public class Test : Action {

		public Test( Unit owner )
			: base( owner, owner, "Test", ActionSubjectType.Self, 3 ) { }

		public override void _Execute( object subject ) {

			Process p;


			p = new ProcessBook.Wait( 10 );
			processQueue += p;

			p = new ProcessBook.WaitSeconds( .25f );
			processQueue += p;

			p = new ProcessBook.ChangeTimeSpeed( .2f, 1f );
			processQueue += p;

			p = new ProcessBook.WaitSeconds( .1f );
			processQueue += p;

			Buff b = new Buff( "Bleeding" );
			b.duration = 100;
			b.TurnStartEvent += delegate { owner.Damage( 2, DamageType.INTERNAL, owner ); };
			owner.buffs.Add( b );

			p = new ProcessBook.ChangeTimeSpeed( 1f, 1f );
			processQueue += p;

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

	public class Move : Action {

		public Move( Unit owner, object source )
			: base( owner, source, "Move", ActionSubjectType.GridTile ) { }

		protected override bool _possible { get { return owner.canMove; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is GridTile && ( subject as GridTile ).selectable;
		}

		public override void _OnSelected() {
			GameMode.Set( GameModes.PickTile );
			processQueue.Add( new ProcessBook.Wait( 2 ) );
			processQueue.Add( new ProcessBook.HighlightWalkableTiles( owner ) );
		}

		public override void _Execute( object subject ) {

			if( subject is GridTile ) {

				Debug.Log( owner + " moving from " + owner.currentTile + " to " + subject );

				Process p;

				//p = new ProcessBook.ChangeTimeSpeed( .2f, 3f );
				//processQueue.Add( p, true );

				p = new ProcessBook.Wait( 10 );
				processQueue.Add( p );

				p = new ProcessBook.UnitMoveAlongPath( owner, subject as GridTile );
				processQueue.Add( p );

				p = new ProcessBook.Wait( 10 );
				processQueue.Add( p );
				p.AttachPassive( new ProcessBook.ChangeTimeSpeed( 1f, .5f ) );

				p.eventEnded += Finish;

			} else
				Debug.LogWarning( name.ToUpper() + " ACTION SUBJECT IS INCORRECT TYPE" );

		}

		public override void OnDeselected() {

			God.grid.ResetTiles();
			GameMode.Reset();


		}

	}

	public class Attack : Action {

		public Attack( Unit owner, Weapon source )
			: base( owner, source, "Attack", ActionSubjectType.Unit ) { }

		protected override bool _possible { get { return owner.canAttack; } }

		public override bool _IsSubjectViable( object subject ) {
			return subject is Unit && owner.CanTarget( subject as Unit ); ;
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

				p = new ProcessBook.WaitSeconds(.15f);
				processQueue += p;

				p = new ProcessBook.ChangeTimeSpeed( 1f );
				processQueue.Add( p );

				p = new ProcessBook.WaitSeconds( .3f );
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

		public Defend( Unit owner, object source )
			: base( owner, source, "Defend" ) { }

		protected override bool _possible { get { return !owner.buffs[ BuffPropFlag.Defending ]; } }

		public override void _Execute( object subject ) {

			Logger.Respond( owner + " defending." );
			owner.buffs.Add( new Buff( "Evasive", BuffPropFlag.Defending ) );

			Finish();

		}

	}

	public class Reload : InstantAction {

		public Reload( Unit owner, Firearm source )
			: base( owner, source, "Reload" ) { }

		protected override bool _possible { get { return ( source as Firearm ).ammoLeft < ( source as Firearm ).ammoClipSize; } }

		public override void _Execute( object subject ) {

			owner.model.Reload();
			( source as Firearm ).Reload();
			Logger.Respond( owner + " reloading." );

			Finish();

		}

	}

}

