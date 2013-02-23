using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon {

	// CHANGABLES
    
    public float chanceBleeding = 100;

	// PROPERTY GETTERS

	public new DamageType damageType;
	public float speed = 100;

	public override void Init( Unit owner ) {
		actions.Add( new ActionsBook.Lunge( owner, this ) );
		base.Init( owner );
	}

	public override void Attack( Unit targetUnit, IDamageable hittee ) {

		if( this.Chance(chanceBleeding) ) {
                
			targetUnit.buffs += BuffBook.Bleeding(targetUnit);
                
		}

	}

	public void OnAttackFinished() {
		eventAttackFinished.Invoke(this);
	}

	public override bool CanTarget( Unit targetUnit ) {
		return true;
	}

}
