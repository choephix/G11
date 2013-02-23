using System.Linq;
using UnityEngine;
using System.Collections.Generic;



[System.Serializable]
public abstract class Buff : object {

	public readonly string name;

	public int duration = 1;

	public bool stackable = true; //TODO test
	public BuffTerminationCondition terminationCondition = BuffTerminationCondition.Timeout;

	public event EventHandler AppliedEvent;
	public event EventHandler RemovedEvent;
	public event EventHandler TurnStartEvent;
	public event EventHandler<int> TurnsPassedEvent = delegate { };

	public event EventHandler TerminatedEvent;

	private int turnsPassed;

	//CONSTRUCTORS

	protected Buff( string name ) {

		this.name = name;

	}

	public void Terminate( object o = null ) {

		Debug.Log( "Buff " + this + " expired" );

		if( TerminatedEvent != null ) TerminatedEvent.Invoke();

	}

	//OTHER STUFF

	public abstract bool GetFlag( BuffPropFlag flag );

	public abstract float GetMultiplier( BuffPropMult prop );

	//HANDLERS

	public void OnApplied() {
		if( AppliedEvent != null ) AppliedEvent.Invoke();
	}

	public void OnRemoved() {
		AppliedEvent = null;
		RemovedEvent = null;
		TerminatedEvent = null;
		TurnStartEvent = null;
		if( RemovedEvent != null ) RemovedEvent.Invoke();
	}

	public void OnTurnStart() {

		if( TurnStartEvent != null ) {
			Debug.Log( this + " doing something on turn start--" );
			TurnStartEvent.Invoke();
		}

		turnsPassed++;
		if( TurnsPassedEvent != null ) TurnsPassedEvent.Invoke( turnsPassed );

	}

	//OPERATORS

	public bool this[BuffPropFlag flag] {
		get { return GetFlag( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetMultiplier( mul ); }
	}

	//OVERRIDES

	public override string ToString() {
		return name;
	}

	//CLASSES

	[System.Serializable]
	public class Mult : object {

		public readonly BuffPropMult prop;
		public readonly float value;

		public Mult( BuffPropMult prop, float value = 1f ) {
			this.prop = prop;
			this.value = value;
		}

		public override string ToString() {
			return prop.ToString();
		}

	}

}

public class BuffBook {

	public static Buff Ducked( Unit buffee ) {

		return new BuffsBook.Ducked( buffee );

	}

	public static Buff Alert( float amount = 1.5f ) {

		return new BuffsBook.StatusEffect( "Alert", new Buff.Mult( BuffPropMult.Evasion, amount ) ) { duration = 1 };

	}

	public static Buff Bleeding( Unit buffee, int amount = 2 ) {

		return new BuffsBook.Bleeding( buffee, amount );

	}

	public static Buff Poisoned( Unit buffee, int damage = 2, int duration = 5 ) {

		return new BuffsBook.Poisoned( buffee, damage, duration );

	}

	public static Buff Intoxicated( Unit buffee, int damage = 1 ) {

		return new BuffsBook.Poisoned( buffee, damage );

	}

}




public class BuffsBook {

	public class StatusEffect : Buff {

		public readonly List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
		public readonly List<Mult> multProps = new List<Mult>();

		//CONSTRUCTORS

		public StatusEffect( string name, Mult multiplier )
			: base( name ) {
			multProps.Add( multiplier );

		}

		public StatusEffect( string name, IEnumerable<BuffPropFlag> flagProps, IEnumerable<Mult> multipliers = null )
			: base( name ) {

			this.flagProps.AddRange( flagProps );

			if( multipliers != null ) {
				multProps.AddRange( multipliers );
			}

		}

		// GETTERS

		public override bool GetFlag( BuffPropFlag flag ) {
			return flagProps.Contains( flag );
		}

		public override float GetMultiplier( BuffPropMult prop ) {
			return multProps.Where( mul => mul.prop == prop ).Aggregate( 1f, ( current, mul ) => current * mul.value );
		}

		// OTHER

		public string ToStringXL() {
			return flagProps.Aggregate( "", ( current, flag ) => current + ( '[' + flag + ']' ) );
		}

	}

	public class WatcherEffect : Buff {

		public WatcherEffect( string name ) : base( name ) {
			


		}

		public override sealed bool GetFlag( BuffPropFlag flag ) {
			return false;
		}

		public override sealed float GetMultiplier( BuffPropMult prop ) {
			return 1f;
		}

	}

	// STATUS EFFECTS

	public sealed class Ducked : StatusEffect {

		private const float HEIGHT_MULTIPLIER = .6f;

		public Ducked( Unit buffee )
			: base( "Ducked", new Buff.Mult( BuffPropMult.Height, HEIGHT_MULTIPLIER ) ) {

			stackable = false;
			terminationCondition = BuffTerminationCondition.NextAction;

			AppliedEvent += delegate {
				buffee.eventActionStarted += Terminate;
				buffee.model.SetPosture( UnitModelPosture.CoverDucked );
			};

			RemovedEvent += delegate {
				buffee.eventActionStarted -= Terminate;
				buffee.model.SetPosture( UnitModelPosture.Normal );
			};
		}

	}

	// OTHER

	public sealed class Poisoned : WatcherEffect {

		public Poisoned( Unit buffee, int amount = 2, int duration = -1 )
			: base( "Poisoned" ) {

			stackable = true;

			if( duration > 0 ) {
				this.duration = duration;
			} else {
				terminationCondition = BuffTerminationCondition.Eternal;
			}

			TurnStartEvent += () => buffee.Damage( amount, DamageType.INTERNAL, buffee );

		}

	}

	// OTHER

	public class ActionClearableBuff : WatcherEffect {

		protected readonly Action clearAction;

		public ActionClearableBuff( string name, Unit buffee, string actionName )
			: base( name ) {

			terminationCondition = BuffTerminationCondition.Eternal;

			clearAction = new ActionsBook.ClearBuff( buffee , actionName , name );

			AppliedEvent += delegate {
				buffee.actions.Add( clearAction );
			};

			RemovedEvent += delegate {
				buffee.actions.Remove( clearAction );
			};

		}

	}

	public sealed class Bleeding : ActionClearableBuff {

		public Bleeding( Unit buffee, int amount = 2 )
			: base( "Bleeding", buffee, "Stitch Up" ) {

			stackable = true;

			TurnStartEvent += () => buffee.Damage( amount, DamageType.INTERNAL, buffee );

		}

	}

	public sealed class Enflamed : ActionClearableBuff {

		public Enflamed( Unit buffee, int amount = 2 )
			: base( "On Fire", buffee, "Drop And Roll" ) {

			stackable = false;

			TurnStartEvent += () => buffee.Damage( amount, DamageType.NORMAL, buffee );

		}

	}




}
