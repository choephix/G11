using UnityEngine;
using System.Collections;

public class Grenade : Throwable {

	[SerializeField]
	protected LineRenderer line;

	public float damage = 8; //TODO privatize
	public DamageType damageType = DamageType.NORMAL;

	[SerializeField]
	protected Damage ddamage;

	public override void Init( Unit owner ) {

		base.Init( owner );

		owner.actions.Add( new ActionsBook.ThrowGrenade( owner, this ) );

	}

}

[System.Serializable]
public class Damage {

	[SerializeField]
	protected float _damage;
	public float damage { get { return _damage; } }

	[SerializeField]
	protected DamageType _damageType;
	public DamageType damageType { get { return _damageType; } }


}

public class AreaDamage : Damage {


	[SerializeField]
	protected float _range;
	public float range { get { return _range; } }


}