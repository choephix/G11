using UnityEngine;
using System.Collections;

public class AnimationSpeedFixer : MonoBehaviour {

	void Update () {
		if( animation.isPlaying ) {
			foreach( AnimationState state in animation ) {
				state.speed = GodOfTime.speed;
			}
		}
	}

}
