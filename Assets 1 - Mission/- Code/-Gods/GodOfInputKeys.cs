using UnityEngine;
using System.Collections;

#if UNITY_STANDALONE_WIN

public class GodOfInputKeys : MonoBehaviour {

	internal static string B_ATTACK	= "Attack";
	internal static string B_NEXT	= "Next";
	internal static string B_PREV	= "Previous";
	internal static string B_WEAPON	= "SwitchWeapon";
	internal static string B_AXIS_H	= "Horizontal";
	internal static string B_AXIS_V	= "Vertical";
	internal static string B_AXIS_ROTATE = "Rotate";
	internal static string B_AXIS_WHEEL	 = "MouseWheel";
	//internal static string B_SWITCH	 = "SwitchUnit";
	internal static string B_CONFIRM	 = "Confirm";
	internal static string B_BACK		 = "Back";
	//internal static string B_CANCEL	 = "Back";
	//internal static string B_MENU		 = "Back";

	internal static string ACTION_1 = "Action1";
	internal static string ACTION_2 = "Action2";
	internal static string ACTION_3 = "Action3";
	internal static string ACTION_4 = "Action4";
	internal static string ACTION_5 = "Action5";
	internal static string ACTION_6 = "Action6";
	internal static string ACTION_7 = "Action7";
	internal static string ACTION_8 = "Action8";
	internal static string ACTION_9 = "Action9";
	internal static string ACTION_0 = "Action0";

	void Update() {

		if( GodOfInteraction.handleUserInput ) {

			if( Input.GetAxis( B_AXIS_H ) != 0 ) {
				GodOfInteraction.OnInput_Horizontal( -Input.GetAxis( B_AXIS_H ) * Time.deltaTime * 6 );
			}
			if( Input.GetAxis( B_AXIS_V ) != 0 ) {
				GodOfInteraction.OnInput_Vertical( -Input.GetAxis( B_AXIS_V ) * Time.deltaTime * 6 );
			}
			if( Input.GetAxis( B_AXIS_ROTATE ) != 0 ) {
				GodOfInteraction.OnInput_Rotary( Input.GetAxis( B_AXIS_ROTATE ) * Time.deltaTime * 6 );
			}
			if( Input.GetAxis( B_AXIS_WHEEL ) != 0 ) {
				GodOfInteraction.OnInput_Wheel( Input.GetAxis( B_AXIS_WHEEL ) * Time.deltaTime );
			}
			if( Input.GetButtonDown( B_CONFIRM ) ) {
				GodOfInteraction.OnInput_Confirm();
			}
			if( Input.GetButtonDown( B_BACK ) ) {
				GodOfInteraction.OnInput_Back();
			}

			if( Input.GetButtonDown( B_ATTACK ) ) {
				GodOfInteraction.OnInput_Attack();
			}
			if( Input.GetButtonDown( B_WEAPON ) ) {
				GodOfInteraction.OnInput_SwitchWeapon();
			}

			if( Input.GetButtonDown( B_NEXT ) ) {
				GodOfInteraction.OnInput_Next();
			}
			if( Input.GetButtonDown( B_PREV ) ) {
				GodOfInteraction.OnInput_Prev();
			}

			///////////////////////////////////////// ACTIONS

			for( int i=0 ; i < 10 ; i++ ) {
				//if( Input.GetButtonDown( "Action__DEPRICATING" + i ) ) {
				if( Input.GetKeyDown( i.ToString() ) ) {
					GodOfInteraction.OnInput_Action( i );
				}
			}

		}

	}
	
}

#endif