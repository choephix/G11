using UnityEngine;
using System.Collections;

public class Medikit : Equippable {

	public int healAmount = 2;

	public override void Init( Unit owner ) {

		base.Init( owner );

		owner.actions.Add( new ActionsBook.Heal( owner, this, healAmount ) );

	}

}
