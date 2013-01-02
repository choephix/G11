using UnityEngine;
using System.Collections;

public class AutoAnimatedMaterial : MonoBehaviour {

	public Vector2 offsetSpeed = Vector2.zero;

	void Update () {
		if( renderer.enabled ) {
			renderer.material.mainTextureOffset = new Vector2( offsetSpeed.x * GodOfTime.time % 1, offsetSpeed.y * GodOfTime.time % 1 );
		}
	}

}
