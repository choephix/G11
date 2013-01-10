using UnityEngine;
using System.Collections;

public class Grenade : Throwable {

	[SerializeField]
	protected LineRenderer line;

	public float range = 6; //TODO privatize
	public float damage = 8; //TODO privatize

	public override void Init( Unit owner ) {

		base.Init( owner );

		owner.actions.Add( new ActionsBook.ThrowGrenade( owner, this ) );

	}

}
