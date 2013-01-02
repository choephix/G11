using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Animation ) )]
public class AnimationSpeedFixer : MonoBehaviour {

	void Update () {
		if( animation.isPlaying ) {
			foreach( AnimationState state in animation ) {
				state.speed = GodOfTime.speed;
			}
		}
	}

}
