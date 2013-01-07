using UnityEngine;
using System.Collections;

public class Grenade : Throwable {

	[SerializeField]
	protected LineRenderer line;

	public override void Init( Unit owner ) {

		base.Init( owner );

		owner.actions.Add( new ActionsBook.ThrowGrenade( owner, this ) );

	}

}
