using UnityEngine;
using System.Collections;

public class GodOfTests : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {

		if( God.selectedUnit ) {
			if( !God.selectedUnit.selectable ) {
				Throw( "Unselectable Unit is selected - " + God.selectedUnit.name );
			}
		} else {
			if( GameMode.interactive ) {
				Throw( "No unit selected" );
			}
		}

		if( God.targetedUnit ) {
			if( !God.targetedUnit.targetable && GameMode.Is( GameModes.PickUnit ) ) {
				Throw( "Untargetable Unit is targeted - " + God.targetedUnit.name );
			}
		}

	}

	private void Throw( string msg ) {

		throw new UnityException( "GOTCHA: " + msg );

	}

}
