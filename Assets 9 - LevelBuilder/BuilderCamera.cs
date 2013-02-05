using UnityEngine;
using System.Collections;

public class BuilderCamera : MonoBehaviour {

	internal const string B_AXIS_H = "Horizontal";
	internal const string B_AXIS_V = "Vertical";
	internal const string B_AXIS_ROTATE = "Rotate";
	internal const string B_AXIS_WHEEL	 = "MouseWheel";

	private const float MOVESPEED_DEFAULT = .005f;
	private const float MOVESPEED_MAX = .025f;

	private const float DISTANCE_MIN =  5;
	private const float DISTANCE_MAX = 32;

	[SerializeField]
	private Camera camera;

	private Vector3 cameraLocalDestination;
	private float moveSpeed = MOVESPEED_DEFAULT; 

	void Start () {

		camera.transform.LookAt( transform.position );
		cameraLocalDestination = camera.transform.localPosition;

	}
	
	void Update () {

		if( Mathf.Abs( Input.GetAxis( B_AXIS_ROTATE ) ) > .01 ) {
			
			transform.localEulerAngles += Vector3.up * Input.GetAxis( B_AXIS_ROTATE ) * 2;

		}

		if( Mathf.Abs( Input.GetAxis( B_AXIS_WHEEL ) ) > .01 ) {

			//camera.transform.position += Vector3.up * Input.GetAxis( B_AXIS_WHEEL ) * MOVESPEED_DEFAULT;
			cameraLocalDestination -= cameraLocalDestination * Input.GetAxis( B_AXIS_WHEEL ) * MOVESPEED_DEFAULT;
			if( cameraLocalDestination.magnitude > DISTANCE_MAX ) {
				cameraLocalDestination = cameraLocalDestination.normalized * DISTANCE_MAX;
			}
			if( cameraLocalDestination.magnitude < DISTANCE_MIN ) {
				cameraLocalDestination = cameraLocalDestination.normalized * DISTANCE_MIN;
			}
			camera.transform.LookAt( transform.position );

		}

		camera.transform.localPosition = Vector3.Lerp( camera.transform.localPosition , cameraLocalDestination , .25f );

		if( Mathf.Abs( Input.GetAxis( B_AXIS_V ) ) > .01 || Mathf.Abs( Input.GetAxis( B_AXIS_H ) ) > .01 ) {

			transform.position += 
				( transform.forward * Input.GetAxis( B_AXIS_V ) + transform.right * Input.GetAxis( B_AXIS_H ) )
				* moveSpeed * cameraLocalDestination.magnitude;

			if( moveSpeed < MOVESPEED_MAX ) {
				moveSpeed += .001f;
			}

		} else {

			moveSpeed = MOVESPEED_DEFAULT;

		}

	}

}
