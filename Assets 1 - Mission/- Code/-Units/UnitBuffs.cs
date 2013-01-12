using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitBuffs {

	private readonly List<Buff> list = new List<Buff>();

	public void OnTurnStart() {

		int i = 0;
		Buff buff;

		while( i < list.Count ) {

			buff = list[i];

			if( buff.eternal ) {

				buff.duration--;

				if( buff.duration <= 0 ) {
					RemoveBuff( buff );
				} else {
					buff.OnTurnStart();
					i++;
				}

			}

		}

	}

	//-- -- -- -- -- 

	public void Add( Buff buff ) {

		if( !buff.stackable ) {
			RemoveDuplicates( buff );
		}

		list.Add( buff );
		buff.OnApplied( );
		buff.TerminatedEvent += delegate { RemoveBuff( buff ); };

	}

	private void RemoveBuff( Buff buff ) {
		list.Remove( buff );
		buff.OnRemoved();
	}

	private void RemoveDuplicates( Buff buff ) {
		RemoveDuplicates( buff.name );
	}
	private void RemoveDuplicates( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		dups.ForEach( RemoveBuff );

	}

	public bool HasDuplicates( Buff buff ) {
		return HasBuff( buff.name );
	}
	public bool HasBuff( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		return dups.Count > 0;

	}

	//-- -- -- -- -- 

	public bool GetFlagProp( BuffPropFlag flag ) {
		bool r = false;
		foreach( Buff buff in list ) {
			r |= buff[flag];
		}
		return r;
	}

	public float GetFlagMult( BuffPropMult mul ) {
		float r = 1f;
		foreach( Buff buff in list ) {
			r *= buff[mul];
		}
		return r;
	}

	//-- -- -- -- -- 

	public override string ToString() {
		string s = "";
		foreach( Buff buff in list ) {
			s += "{" + buff + "}";
		}
		return s;
	}

	public bool this[BuffPropFlag flag] {
		get { return GetFlagProp( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetFlagMult( mul ); }
	}

	public static UnitBuffs operator +( UnitBuffs bm, Buff b ) { bm.Add( b ); return bm; }

	public BuffsBook factory;

}

[System.Serializable]
public class Buff : object {

	public string name;

	public int duration = 1;

	public bool stackable = true; //TODO test
	public bool eternal = true;

	public BuffPropFlag[] flags;
	public Mult[] mults;

	public readonly List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
	public readonly List<Mult> multProps = new List<Mult>();

	public event EventHandler AppliedEvent;
	public event EventHandler RemovedEvent;
	public event EventHandler TerminatedEvent;
	public event EventHandler TurnStartEvent;
	public event EventHandler<int> TurnsPassedEvent = delegate { };

	private int turnsPassed = 0;

	//CONSTRUCTORS

	public Buff( string name, Mult multiplier )
		: this( name ) {
		this.multProps.Add( multiplier );
	}

	public Buff( string name, BuffPropFlag flagProp, Mult multiplier = null )
		: this( name ) {

		this.flagProps.Add( flagProp );

		if( multiplier != null ) {
			this.multProps.Add( multiplier );
		}

	}

	public Buff( string name, BuffPropFlag[] flagProps, Mult[] multipliers = null )
		: this( name ) {

		this.flagProps.AddRange( flagProps );

		if( multipliers != null ) {
			this.multProps.AddRange( multipliers );
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
		float r = 1f;
		foreach( Mult mul in multProps ) {
			if( mul.prop == prop ) {
				r *= mul.value;
			}
		}
		return r;
	}

	public void Terminate() {
		if( TerminatedEvent != null ) TerminatedEvent.Invoke();
	}

	//HANDLERS

	public void OnTurnStart() {

		TurnStartEvent.Invoke();

		turnsPassed++;
		TurnsPassedEvent.Invoke( turnsPassed );

	}

	public void OnApplied() {
		if( AppliedEvent != null ) AppliedEvent.Invoke();
	}

	public void OnRemoved() {
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
		string s = "";
		foreach( BuffPropFlag flag in flagProps ) {
			s += '[' + flag + ']';
		}
		return s;
	}

	public override string ToString() {
		return name;
	}

	//CLASSES

	[System.Serializable]
	public class Mult : object {

		public BuffPropMult prop;
		public float value;

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


		Buff b = new Buff( "Ducked", new Buff.Mult( BuffPropMult.Height, .5f ) ) { eternal = true };

		b.AppliedEvent += delegate( ) { buffee.model.SetPosture( UnitModelPosture.CoverDucked ); };
		b.RemovedEvent += delegate() { buffee.model.SetPosture( UnitModelPosture.Normal ); };

		buffee.eventActionStarted += delegate( Action a ) { b.Terminate(); };

		return b;

	}

	public static Buff Alert( Unit buffee, float amount = .5f ) {

		return new Buff( "Alert", new Buff.Mult( BuffPropMult.Evasion, amount ) ) { duration = 1 };

	}

	public static Buff Bleeding( Unit buffee, int amount = 2 ) {

		Buff b = new Buff( "Bleeding" ) { eternal = true, };
		b.TurnStartEvent += () => buffee.Damage( amount, DamageType.INTERNAL, buffee );
		return b;

	}

	public static Buff Poisoned( Unit buffee, int duration = 5 ) {

		Buff b = new Buff( "Poisoned" ) { duration = duration };
		b.TurnStartEvent += () => buffee.Damage( 1, DamageType.INTERNAL, buffee );
		return b;

	}

}
















