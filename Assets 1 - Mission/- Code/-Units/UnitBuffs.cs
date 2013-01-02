using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitBuffs {

	private List<Buff> buffs = new List<Buff>();

	public void Add( Buff buff ) {
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

public enum BuffPropFlag { Defending }
public enum BuffPropMult { Accuracy, RangedDamage, MeleeDamage, Evasion }

public class Buff {

	public int duration = 1;

	public bool stackable = true; //TODO implement
	public bool eternal = true;

	public List<BuffPropFlag> flagProps = new List<BuffPropFlag>();
	public List<Mult> multProps = new List<Mult>();

	//CONSTRUCTORS

	public Buff( Mult multiplier = null ) {
		this.multProps.Add( multiplier );
	}

	public Buff( BuffPropFlag flagProp ) {
		this.flagProps.Add( flagProp );
	}

	public Buff( Mult[] multipliers = null ) {
		this.multProps.AddRange( multipliers );
	}

	public Buff( BuffPropFlag[] flagProps ) {
		this.flagProps.AddRange( flagProps );
	}

	public Buff( BuffPropFlag[] flagProps, Mult[] multipliers ) {
		this.flagProps.AddRange( flagProps );
		this.multProps.AddRange( multipliers );
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

	public bool this[BuffPropFlag flag] {
		get { return GetFlag( flag ); }
	}

	public float this[BuffPropMult mul] {
		get { return GetMultiplier( mul ); }
	}

	//VIRTUAL

	public override string ToString() {
		string s = "";
		foreach( BuffPropFlag flag in flagProps ) {
			s += "[" + flag + "]";
		}
		return s;
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