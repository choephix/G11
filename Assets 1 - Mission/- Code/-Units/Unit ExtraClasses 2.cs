using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class Damage {

	public readonly float amount;
	public readonly DamageType type;

	public readonly Buff[] buffs;

	public Damage( float amount, DamageType type, Buff[] buffs ) {
		this.amount = amount;
		this.type = type;
		this.buffs = buffs;
	}

}

[System.Serializable]
public class DamageData {


	public float amount;
	public DamageType type;

	public float chanceBleeding;
	public float chanceEnflamed;
	public float chanceStunned;



	public Damage FabricateDamage() {

		List<Buff> buffs = new List<Buff>();

		return new Damage( amount, type, buffs.ToArray() );

	}

}