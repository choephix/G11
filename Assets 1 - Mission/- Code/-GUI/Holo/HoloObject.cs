using UnityEngine;
using System.Collections;

public class HoloObject : MissionBaseClass {

	public Renderer model;

	public Renderer[] renderers;

	public bool visible { set { 
		foreach( Renderer renderer in renderers ) renderer.enabled = value; } }
	public new bool active { set { 
			//foreach( Renderer renderer in renderers ) renderer.gameObject.active = value; 
			gameObject.SetActive( value ); } }

}
