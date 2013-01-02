using UnityEngine;
using System.Collections;

public class WorldObject : BaseClass {
	
//	public const float GRAVITY = 10;
//	public float mass = 1.0f;

	public Renderer model;
	
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

	private Color materialColor;
	internal float alpha {
		get { return model.material.color.a; }
		set {
			materialColor = model.material.GetColor( "_Color" );
			materialColor.a = value;
			model.material.SetColor( "_Color", materialColor );
		}
	}
	
	
}
