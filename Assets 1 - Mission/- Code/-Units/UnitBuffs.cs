using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitBuffs {

	private readonly List<Buff> list = new List<Buff>();

	public void OnTurnStart() {

		int i = 0;
		Buff buff;
		bool expired;

		while( i < list.Count ) {

			buff = list[i];
			expired = false;

			if( buff.terminationCondition == BuffTerminationCondition.Timeout ) {

				buff.duration--;

				if( buff.duration <= 0 ) {

					Debug.Log( "Buff " + buff + " timed out and will be removed." );

					expired = true;

				}

			}

			if( expired ) {

				Remove( buff );

			} else {

				i++;

				buff.OnTurnStart();

			}

		}

	}

	//-- -- -- -- -- 

	public void Add( Buff buff ) {

		if( !buff.stackable ) {
			RemoveDuplicates( buff );
		}

		list.Add( buff );
		buff.OnApplied();
		buff.TerminatedEvent += () => Remove( buff );

	}

	private void Remove( Buff buff ) {
		buff.OnRemoved();
		list.Remove( buff );
	}

	public void Remove( string name ) {
		list.ForEach( delegate( Buff b ) { if( b.name.Equals( name ) ) Remove( b ); } );
	}

	private void RemoveDuplicates( Buff buff ) {
		Remove( buff.name );
	}

	//private void RemoveDuplicates( string name ) {

	//	//List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
	//	//dups.ForEach( Remove );

	//}

	public bool HasDuplicates( Buff buff ) {
		return HasBuff( buff.name );
	}
	public bool HasBuff( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		return dups.Count > 0;

	}

	//-- -- -- -- -- 

	public bool GetFlagProp( BuffPropFlag flag ) {
		return list.Aggregate( false, ( current, buff ) => current | buff[flag] );
	}

	public float GetFlagMult( BuffPropMult mul ) {
		return list.Aggregate( 1f, ( current, buff ) => current * buff[mul] );
	}

	//-- -- -- -- -- 

	public override string ToString() {
		return list.Aggregate( "", ( current, buff ) => current + ( "{" + buff + '}' ) );
	}

	public bool this[BuffPropFlag flag] {
		get { return GetFlagProp( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetFlagMult( mul ); }
	}

	public static UnitBuffs operator +( UnitBuffs bm, Buff b ) { bm.Add( b ); return bm; }

}

[System.Serializable]
public class Buff : object {

	public readonly string name;

	public int duration = 1;

	public bool stackable = true; //TODO test
	public BuffTerminationCondition terminationCondition = BuffTerminationCondition.Timeout;

	public readonly List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
	public readonly List<Mult> multProps = new List<Mult>();

	public event EventHandler AppliedEvent;
	public event EventHandler RemovedEvent;
	public event EventHandler TerminatedEvent;
	public event EventHandler TurnStartEvent;
	public event EventHandler<int> TurnsPassedEvent = delegate { };

	private int turnsPassed;

	//CONSTRUCTORS

	public Buff( string name, Mult multiplier )
		: this( name ) {
		multProps.Add( multiplier );
	}

/*
	public Buff( string name, BuffPropFlag flagProp, Mult multiplier = null )
		: this( name ) {

		flagProps.Add( flagProp );

		if( multiplier != null ) {
			multProps.Add( multiplier );
		}

	}
*/

	public Buff( string name, IEnumerable<BuffPropFlag> flagProps, IEnumerable<Mult> multipliers = null )
		: this( name ) {

		this.flagProps.AddRange( flagProps );

		if( multipliers != null ) {
			multProps.AddRange( multipliers );
		}

	}

	public Buff( string name ) {

		this.name = name;

	}


	//OTHER STUFF

	public bool GetFlag( BuffPropFlag flag ) {
		return flagProps.Contains( flag );
	}

	public float GetMultiplier( BuffPropMult prop ) {
		return multProps.Where( mul => mul.prop == prop ).Aggregate( 1f, ( current, mul ) => current * mul.value );
	}

	public void Terminate() {
		if( TerminatedEvent != null ) TerminatedEvent.Invoke();
	}

	//HANDLERS

	public void OnTurnStart() {

		if( TurnStartEvent != null ) {
			Debug.Log( this + " doing something on turn start--" );
			TurnStartEvent.Invoke();
		}

		turnsPassed++;
		if( TurnsPassedEvent != null ) TurnsPassedEvent.Invoke( turnsPassed );

	}

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

	//OPERATORS

	public bool this[BuffPropFlag flag] {
		get { return GetFlag( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetMultiplier( mul ); }
	}

	//VIRTUAL

	public Buff Clone() {
		return new Buff( name, flagProps.ToArray(), multProps.ToArray() );
	}

	public string ToLongString() {
		return flagProps.Aggregate( "", ( current, flag ) => current + ( '[' + flag + ']' ) );
	}

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

public class BuffsBook {

	public static Buff Ducked( Unit buffee ) {


		Buff b = new Buff( "Ducked", new Buff.Mult( BuffPropMult.Height, .5f ) ) {
			stackable = false,
			terminationCondition = BuffTerminationCondition.NextAction
		};

		b.AppliedEvent += delegate { buffee.model.SetPosture( UnitModelPosture.CoverDucked ); };
		b.RemovedEvent += delegate { buffee.model.SetPosture( UnitModelPosture.Normal ); };

		buffee.eventActionStarted += delegate( Action a ) {
			Debug.LogWarning( buffee + " terminated their DUCKED buff by starting a new action - " + a );
			b.Terminate();
		};

		return b;

	}

	public static Buff Alert( float amount = .5f ) {

		return new Buff( "Alert", new Buff.Mult( BuffPropMult.Evasion, amount ) ) { duration = 1 };

	}

	public static Buff Bleeding( Unit buffee, int amount = 2 ) {

		Buff b = new Buff( "Bleeding" );
		b.terminationCondition = BuffTerminationCondition.Eternal;
		b.TurnStartEvent += () => buffee.Damage( amount, DamageType.INTERNAL, buffee );
		return b;

	}

	public static Buff Poisoned( Unit buffee, int duration = 5 ) {

		Buff b = new Buff( "Poisoned" ) { duration = duration };
		b.TurnStartEvent += () => buffee.Damage( 1, DamageType.INTERNAL, buffee );
		return b;

	}

}
















