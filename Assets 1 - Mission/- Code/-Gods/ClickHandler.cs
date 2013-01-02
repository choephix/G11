using UnityEngine;
using System.Collections;

public class ClickHandler {

	internal static void Up( GridTile tile ) {
		GodOfInteraction.OnPick_Tile( tile );
	}

	internal static void Up( Unit unit ) {
		if( unit.inPlay ) {
			GodOfInteraction.OnPick_Unit( unit );
		}
	}

	internal static void Up( VoidObject voidObject ) {
	}

}
