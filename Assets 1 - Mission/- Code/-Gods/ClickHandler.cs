using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ClickHandler {

	private const bool EDITOR_SHIT = false;

	internal static void Up( GridTile tile ) {
		GodOfInteraction.OnPick_Tile( tile );
#if UNITY_EDITOR
		if( EDITOR_SHIT && tile.currentUnit != null ) {
			Selection.objects = new Object[] { tile.currentUnit.gameObject };
		}
#endif
	}

	internal static void Up( Unit unit ) {
		if( unit.inPlay ) {
			GodOfInteraction.OnPick_Unit( unit );
		}
#if UNITY_EDITOR
		if( EDITOR_SHIT ) {
			Selection.objects = new Object[] {unit.gameObject};
		}
#endif
	}

	internal static void Up( VoidObject voidObject ) {
	}

}
