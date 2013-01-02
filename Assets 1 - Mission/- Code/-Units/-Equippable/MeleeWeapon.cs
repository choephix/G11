using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon {

	// CHANGABLES

	// PROPERTY GETTERS

	public new DamageType damageType;
	public float speed = 100;

	public override void Attack( Unit targetUnit, bool hit ) {

		StartCoroutine( AttackCoroutine() );

	}

	public IEnumerator AttackCoroutine() {
		yield return new WaitForSeconds( .15f );
		//	OnAttackFinished();
	}

	public void OnAttackFinished() {
		eventAttackFinished.Invoke(this);
		//	d_OnAttackFinished( this );
	}

	public override bool CanTarget( Unit targetUnit ) {
		return true;
	}

}
