using UnityEngine;
using System.Collections;
using System.Text;

public class UnitBillboard : HoloObject {

	public TextMesh label;
	public TextMesh labelBig;

	private StringBuilder sb;

	private bool _visible;
	public new bool visible {
		get { return _visible; }
		set {
			if( _visible == value ) return;
			_visible = value;
			if( value ) {
				AnimateIn();
			} else {
				AnimateOut();
			}
		}
	}

	void Start() {
		Hide();
	}

	public void UpdateLabels( Unit unit ) {

		labelBig.renderer.material.color =
		label.renderer.material.color = unit.team.color;

		sb = new StringBuilder();

		sb.AppendLine( unit.props.unitName );
		//for( byte i=0 ; i < unit.props.maxHealth ; i++ ) {
		//    sb.Append( i < unit.status.health ? "[]" : " -" );
		//}
		//for( byte i=0 ; i < unit.status.armor ; i++ ) {
		//    sb.Append( "{}" );
		//}
		for( byte i=0 ; i < unit.props.maxHealth ; i++ ) {
			sb.Append( i < unit.status.health ? 'O' : '-' );
		}
		for( byte i=0 ; i < unit.status.armor ; i++ ) {
			sb.Append( '0' );
		}

		label.text = sb.ToString();

		if( GameMode.Is( GameModes.PickUnit ) && unit.targetable ) {
		//	transform.localScale = Vector3.one;
			label.fontSize = 40;
			labelBig.text = Mathf.Round( selectedUnit.relations.GetAttackResult( unit ).hitChance ).ToString() + '%';
		} else {
		//	transform.localScale = Vector3.one * .5f;
			label.fontSize = 20;
			labelBig.text = unit.currentTile.CalculateDanger( unit ).ToPercent(0);
		}

	}

	private void AnimateIn() {
		label.renderer.enabled = true;
		labelBig.renderer.enabled = true;
		animation.Play( "animateIn" );
	}

	private void AnimateOut() {
		animation.Play( "animateOut" );
	}

	private void Hide() {
		label.renderer.enabled = false;
		labelBig.renderer.enabled = false;
	}

}
