using UnityEngine;
using System.Collections;

public class UnitAnimator : MonoBehaviour {

	protected Animator animator;

	public const string CROUCH	= "crouch";
	public const string DEAD	= "dead";
	public const string DAMAGE	= "damage";
	public const string RELOAD	= "reload";
	public const string SHOOT	= "shoot";
	public const string AIMING	= "aiming";
	public const string MOVING	= "moving";
	public const string JUMPOVER= "jumpover";

	void Awake() {

		animator = GetComponent<Animator>();

		animator.SetLayerWeight( 1, 1 );

	}

	public bool crouching { set { animator.SetBool( "crounch", value ); } }
	public bool dead { set { animator.SetBool( "dead", value ); } }
	public bool damage { set { animator.SetBool( "damage", value ); } }
	public bool reload { set { animator.SetBool( "reload", value ); } }
	public bool shoot { set { animator.SetBool( "shoot", value ); } }
	public bool aiming { set { animator.SetBool( "aiming", value ); } }
	public bool moving { set { animator.SetFloat( "movementSpeed", value ? 1f : 0f ); } }

	public void QuickSetRevert( string param ) {

		Debug.Log( "setting param "+param );

		animator.SetBool( param, true );

		StartCoroutine( RevertParam(param) );


	}

	private IEnumerator RevertParam( string param ) {
		yield return new WaitForSeconds( .1f );
		animator.SetBool( param, false );

		Debug.Log( "reverted param " + param );
	}

	void Update() {

		//if( animator ) {

		//	//get the current state
		//	AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo( 0 );

		//	//if we're in "Run" mode, respond to input for jump, and set the Jump parameter accordingly. 
		//	if( stateInfo.nameHash == Animator.StringToHash( "Base Layer.RunBT" ) ) {
		//		if( Input.GetButton( "Fire1" ) )
		//			animator.SetBool( "Jump", true );
		//	} else {
		//		animator.SetBool( "Jump", false );
		//	}

		//	float h = Input.GetAxis( "Horizontal" );
		//	float v = Input.GetAxis( "Vertical" );

		//	//set event parameters based on user input
		//	animator.SetFloat( "Speed", h * h + v * v );
		//	animator.SetFloat( "Direction", h, DirectionDampTime, Time.deltaTime );

		//}

	}

}