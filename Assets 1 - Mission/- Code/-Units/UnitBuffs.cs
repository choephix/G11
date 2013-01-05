using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitBuffs {

	private List<Buff> buffs = new List<Buff>();

	public void Add( Buff buff ) {

		if( !buff.stackable ) {

		}

		buffs.Add( buff );

	}

	public void OnTurnStart() {

		int i = 0;
		Buff buff;
		
		while( i < buffs.Count ) {

			buff = buffs[i];

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

	private void RemoveBuff( Buff buff ) {
		buffs.Remove( buff );
	}

	public bool GetFlagProp( BuffPropFlag flag ) {
		bool r = false;
		foreach( Buff buff in buffs ) {
			r |= buff[ flag ];
		}
		return r;
	}

	public float GetFlagMult( BuffPropMult mul ) {
		float r = 1f;
		foreach( Buff buff in buffs ) {
			r *= buff[ mul ];
		}
		return r;
	}

	public override string ToString() {
		string s = "";
		foreach( Buff buff in buffs ) {
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

}

public class Buff : object {

	public readonly string name;

	public int duration = 1; //TODO is this implemented?

	public bool stackable = true; //TODO test
	public bool eternal = true;

	public readonly List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
	public readonly List<Mult> multProps = new List<Mult>();

    public event EventHandler TurnStartEvent;
    public event EventHandler<int> TurnsPassedEvent;
    
    private int turnsPassed = 0;
    
	//CONSTRUCTORS

	public Buff( string name, BuffPropFlag flagProp, Mult multiplier = null ) : this( name ) {

		this.flagProps.Add( flagProp );

		if( multiplier!=null ) {
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

	public class Mult {
		
		public BuffPropMult prop;
		public float value;

		public Mult( BuffPropMult prop, float value = 1f ) {
			this.prop = prop;
			this.value = value;
		}

	}

}

public static class CommonBuffs {
    
    public static Buff Bleeding( Damageable buffee ) {
        
    	Buff b = new Buff( "Bleeding" );
		b.eternal = true;
		b.TurnStartEvent += delegate { buffee.Damage( 2, DamageType.INTERNAL, buffee ); };
		owner.buffs.Add( b );
        
    }
    
    public static Buff Poisoned( Damageable buffee ) {
        
    	Buff b = new Buff( "Poisoned" );
		b.duration = 10;
		b.TurnStartEvent += delegate { buffee.Damage( 1, DamageType.INTERNAL, buffee ); };
		owner.buffs.Add( b );
        
    }
    
}
















