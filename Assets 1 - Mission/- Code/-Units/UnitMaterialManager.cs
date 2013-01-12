using System;
using UnityEngine;

public class UnitMaterialManager {

	private Renderer renderer;

	public UnitMaterialManager( UnitModel unitModel ) {

		Init( unitModel );

	}

	public void Init( UnitModel model ) {

		renderer = model.meshRenderer;

		SetMode( UnitMaterialMode.Normal );

	}




	public void SetMode( UnitMaterialMode mode ) {

		Color c = new Color( 0, 0, 0, .25f );

		switch( mode ) {

			case UnitMaterialMode.Targeted:
				c = new Color( 1f, .1f, .0f, .33f );
				break;
			case UnitMaterialMode.Selected:
				c = new Color( .2f, .7f, 1f, .33f );
				break;
			case UnitMaterialMode.HoverSelectable:
				c = new Color( .1f, .5f, 1f, .25f );
				break;
			case UnitMaterialMode.HoverTargetable:
				c = new Color( 1f, .5f, .1f, .25f );
				break;

		}

		renderer.material.SetColor( "_OutlineColor", c );
		//renderer.material.SetFloat( "_Outline", .005f );
		renderer.material.SetFloat( "_Outline", mode.Equals( UnitMaterialMode.Normal ) ? .0033f : .002f );

	}

}

public enum UnitMaterialMode { Targeted, Selected, Normal, HoverSelectable, HoverTargetable }