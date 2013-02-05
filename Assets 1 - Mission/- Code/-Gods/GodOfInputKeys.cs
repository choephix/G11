using System;
using UnityEngine;
using System.Collections;

#if UNITY_STANDALONE_WIN

public class GodOfInputKeys : MonoBehaviour {

	internal const string B_ATTACK	= "Attack";
	internal const string B_NEXT	= "Next";
	internal const string B_PREV	= "Previous";
	internal const string B_WEAPON	= "SwitchWeapon";
	internal const string B_AXIS_H	= "Horizontal";
	internal const string B_AXIS_V	= "Vertical";
	internal const string B_AXIS_ROTATE = "Rotate";
	internal const string B_AXIS_WHEEL	 = "MouseWheel";
	//internal const string B_SWITCH	 = "SwitchUnit";
	internal const string B_CONFIRM	 = "Confirm";
	internal const string B_BACK		 = "Back";

	internal const string B_PAUSE	 = "Pause";
	//internal const string B_MENU		 = "Back";

	internal const string ACTION_1 = "Action1";
	internal const string ACTION_2 = "Action2";
	internal const string ACTION_3 = "Action3";
	internal const string ACTION_4 = "Action4";
	internal const string ACTION_5 = "Action5";
	internal const string ACTION_6 = "Action6";
	internal const string ACTION_7 = "Action7";
	internal const string ACTION_8 = "Action8";
	internal const string ACTION_9 = "Action9";
	internal const string ACTION_0 = "Action0";


	private Vector2 movementVector;

	void Update() {

		if( GodOfInteraction.handleUserInputForCamera ) {


			if( Input.GetButtonDown( B_PAUSE ) ) {
				GodOfTime.speed = GodOfTime.speed == 1f ? .001f : 1f;
			}

			if( Mathf.Abs( Input.GetAxis( B_AXIS_H ) ) > .01 || Mathf.Abs( Input.GetAxis( B_AXIS_V ) ) > .01 ) {
				movementVector.Set( Input.GetAxis( B_AXIS_H ) , Input.GetAxis( B_AXIS_V ) );
				GodOfInteraction.OnInput_Directional( movementVector * Time.deltaTime );
			}


			if( Mathf.Abs( Input.GetAxis( B_AXIS_ROTATE ) ) > .01 ) {
				GodOfInteraction.OnInput_Rotary( Input.GetAxis( B_AXIS_ROTATE ) * Time.deltaTime );
			}
			if( Mathf.Abs( Input.GetAxis( B_AXIS_WHEEL ) ) > .01 ) {
				GodOfInteraction.OnInput_Wheel( Input.GetAxis( B_AXIS_WHEEL ) );
			}

		}

		if( GodOfInteraction.handleUserInput ) {
			
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

			for( int i = 0 ; i < 10 ; i++ ) {
				if( Input.GetKeyDown( i.ToString() ) ) {
					GodOfInteraction.OnInput_Action( i );
				}
			}

		}


		if( Input.GetButtonDown( "Blood&Gore" ) ) {
			Config.GORE = !Config.GORE;
			Logger.Respond( "Blood&Gore turned " + ( Config.GORE ? "ON" : "OFF" ) );
		}

	}
	
}

#endif