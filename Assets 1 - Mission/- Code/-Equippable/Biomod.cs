using UnityEngine;
using System.Collections;

public class Biomod : Equippable {

	public Buff buff;

	void Start() {

	}
	
	public override void Init( Unit owner ) {
		base.Init( owner );
		//actions.Add( new ActionsBook.Attack( owner, this ) );
		if( buff != null ) {
			buff.terminationCondition = BuffTerminationCondition.Eternal;
		}
	//	owner.buffs.Add( buff.Clone() );
		//owner.buffs.Add( buff );
	}

}
