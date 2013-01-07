using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ClickHandler {

	internal static void Up( GridTile tile ) {
		GodOfInteraction.OnPick_Tile( tile );
#if UNITY_EDITOR
		if( tile.currentUnit!=null ) {
			Selection.objects = new GameObject[] { tile.currentUnit.gameObject };
		}
#endif
	}

	internal static void Up( Unit unit ) {
		if( unit.inPlay ) {
			GodOfInteraction.OnPick_Unit( unit );
		}
#if UNITY_EDITOR
		Selection.objects = new GameObject[] { unit.gameObject };
#endif
	}

	internal static void Up( VoidObject voidObject ) {
	}

}
