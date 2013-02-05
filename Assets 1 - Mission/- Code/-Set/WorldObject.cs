using UnityEngine;
using System.Collections;

public class WorldObject : BaseClass {
	
//	public const float GRAVITY = 10;
	//	public float mass = 1.0f;

	public Renderer[] renderers;
	
    internal float x {
          get{ return transform.position.x;  }
          set{ transform.position = new Vector3(x,transform.position.y,transform.position.z);  }
    }
    internal float y {
          get{ return transform.position.y;  }
          set{ transform.position = new Vector3(transform.position.x,y,transform.position.z);  }
    }
    internal float z {
          get{ return transform.position.z;  }
          set{ transform.position = new Vector3(transform.position.x,transform.position.y,z);  }
    }
	
    internal float rotationX {
          get{ return transform.localEulerAngles.x;  }
          set{ transform.localEulerAngles = new Vector3(x,transform.localEulerAngles.y,transform.localEulerAngles.z);  }
    }
    internal float rotationY {
          get{ return transform.localEulerAngles.y;  }
          set{ transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,y,transform.localEulerAngles.z);  }
    }
    internal float rotationZ {
          get{ return transform.localEulerAngles.z;  }
          set{ transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,transform.localEulerAngles.y,z);  }
    }


	private bool _visible = true;
	public bool visible {
		get { return _visible; }
		set {
			_visible = value;
			foreach( Renderer r in renderers ) {
				r.enabled = value;
			}
		}
	}

	private Color tempMaterialColor;
	internal float alpha {
		get { return renderers[0].material.color.a; }
		set {
			foreach( Renderer r in renderers ) {
				foreach( Material m in r.materials ) {
					tempMaterialColor = m.GetColor( "_Color" );
					tempMaterialColor.a = value;
					m.SetColor( "_Color", tempMaterialColor );
				}
			}
		}
	}
	
	
}
