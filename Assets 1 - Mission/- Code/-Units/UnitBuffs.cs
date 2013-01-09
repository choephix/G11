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

	}

	private void RemoveBuff( Buff buff ) {
		list.Remove( buff );
	}

	private void RemoveDuplicates( Buff buff ) {
		RemoveDuplicates( buff.name );
	}
	private void RemoveDuplicates( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		dups.ForEach( b => RemoveBuff( b ) );

	}

	public bool HasDuplicates( Buff buff ) {
		return HasDuplicates( buff.name );
	}
	public bool HasDuplicates( string name ) {

		List<Buff> dups = list.FindAll( b => b.name.Equals( name ) );
		return dups.Count > 0;

	}

	//-- -- -- -- -- 

	public bool GetFlagProp( BuffPropFlag flag ) {
		bool r = false;
		foreach( Buff buff in list ) {
			r |= buff[ flag ];
		}
		return r;
	}

	public float GetFlagMult( BuffPropMult mul ) {
		float r = 1f;
		foreach( Buff buff in list ) {
			r *= buff[ mul ];
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

	public int duration = 1; //TODO is this implemented?

	public bool stackable = true; //TODO test
	public bool eternal = true;

	public BuffPropFlag[] flags;
	public Mult[] mults;

	public readonly List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
	public readonly List<Mult> multProps = new List<Mult>();

    public event EventHandler TurnStartEvent;
	public event EventHandler<int> TurnsPassedEvent = delegate { };
    
    private int turnsPassed = 0;

	//CONSTRUCTORS

	public Buff( string name, Mult multiplier )	: this( name ) {
		this.multProps.Add( multiplier );
	}

	public Buff( string name, BuffPropFlag flagProp, Mult multiplier = null ) : this( name ) {

		this.flagProps.Add( flagProp );

		if( multiplier != null ) {
			this.multProps.Add( multiplier );
		}

	}

	public Buff( string name, BuffPropFlag[] flagProps, Mult[] multipliers = null ) : this( name ) {

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

	public void OnTurnStart() {
        
		TurnStartEvent.Invoke();
        
        turnsPassed++;
        TurnsPassedEvent.Invoke(turnsPassed);
        
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

	public static Buff Alert( Unit buffee, float amount = .5f ) {

		Buff b = new Buff( "Alert", new Buff.Mult( BuffPropMult.Evasion, amount ) );
		b.duration = 1;
		return b;

	}

	public static Buff Bleeding( Unit buffee, int amount = 2 ) {

		Buff b = new Buff( "Bleeding" );
		b.eternal = true;
		b.TurnStartEvent += delegate { buffee.Damage( amount, DamageType.INTERNAL, buffee ); };
		return b;

	}

	public static Buff Poisoned( Unit buffee, int duration = 5 ) {
        
    	Buff b = new Buff( "Poisoned" );
		b.duration = duration;
		b.TurnStartEvent += delegate { buffee.Damage( 1, DamageType.INTERNAL, buffee ); };
		return b;
        
    }
    
}
















