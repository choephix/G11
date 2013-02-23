using UnityEngine;
using System.Collections;

public class FreeCameraController : BaseClass {

	private TransformSpot cameraSpot;
	private Transform cameraTarget;

	private const float DISTANCE_MIN =  .5f;
	private const float DISTANCE_MAX = 2.5f;

	private const float MOVESPEED_DEFAULT = 10f;
	private const float MOVESPEED_MAX = 2f;

	private float moveSpeed = MOVESPEED_DEFAULT;
	private float zoom = 2;

	private Vector3 cameraLocalDestination;
	private float cameraDistance { get { return cameraSpot.transform.localPosition.magnitude; } }

	void Awake() {

		cameraSpot = transform.Find( "CameraSpot" ).GetComponent<TransformSpot>();
		cameraTarget = transform.Find( "CameraTarget" );

	}

	void Start() {

		cameraLocalDestination = cameraSpot.transform.position;

		cameraSpot.Attach( Camera.mainCamera.transform );

	}

	void Update() {

		//cameraSpot.transform.localPosition =
		//	Vector3.Lerp( cameraSpot.transform.localPosition,
		//	3 * new Vector3( 0, zoom * zoom, -zoom ),
		//	.25f );
		cameraSpot.transform.localPosition =
			Vector3.Lerp( cameraSpot.transform.localPosition,
			3 * new Vector3( 0, zoom * 1.25f , -zoom ),
			.25f );
		cameraSpot.transform.LookAt( transform.position );


		
	}

	public void InputMove( Vector2 delta ) {

		transform.position +=
			( transform.forward * delta.y + transform.right * delta.x )
			* moveSpeed * zoom ;

		//if( moveSpeed < MOVESPEED_MAX ) {
		//	moveSpeed += .001f;
		//}

	}

	public void InputZoom( float delta ) {

		zoom -= delta * .005f;
		if( zoom > DISTANCE_MAX ) zoom = DISTANCE_MAX;
		if( zoom < DISTANCE_MIN ) zoom = DISTANCE_MIN;

	}

	public void InputRotate( float delta ) {

		transform.localEulerAngles += Vector3.up * delta * 200;

	}

	//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--

	void MoveTo( Transform destination ) {
		transform.position = destination.position;
	}
	private void MoveTo( Vector3 destination ) {
		transform.position = destination;
	}

}
