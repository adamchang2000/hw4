using UnityEngine;
using System.Collections.Generic;
using wvr;
using WaveVR_Log;

class Point {
	public Vector3 p;
	public Point next;
}

public class LinesGR : MonoBehaviour {

	public Shader shader;

	private Mesh ml;
	private Material lmat;
	
	private Vector3 s;

	private float lineSize = 0.03f;
	
	private GUIStyle labelStyle;
	private GUIStyle linkStyle;
	
	private Point first;
	
	private float speed = 5.0f;

    WaveVR_Controller.Device c_dom;
    private bool _prev;
    Transform dom_transform;


    void Start () {

        c_dom = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant);
        _prev = false;
        dom_transform = transform.GetChild(3);

        labelStyle = new GUIStyle();
		labelStyle.normal.textColor = Color.black;
		
		linkStyle = new GUIStyle();
		linkStyle.normal.textColor = Color.blue;
		
		ml = new Mesh();
		lmat = new Material(shader);
		lmat.color = new Color(0,0,0,0.3f);

	}

	void Update() {
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Trigger);
        if (_press && !_prev)
        {
            first.p = dom_transform.position;
            ml = new Mesh();
        }

        Vector3 e;

        if (Input.GetMouseButtonDown(0))
        {
            s = transform.InverseTransformPoint(GetNewPoint());
        }

        if (Input.GetMouseButton(0))
        {
            e = GetNewPoint();
            AddLine(ml, MakeQuad(transform.TransformPoint(s), e, lineSize), true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            e = GetNewPoint();
            AddLine(ml, MakeQuad(transform.TransformPoint(s), e, lineSize), false);
        }

        Draw();
    }
	
	void Draw() {
		Graphics.DrawMesh(ml, transform.localToWorldMatrix, lmat, 0);
	}
	
	Vector3[] MakeQuad(Vector3 s, Vector3 e, float w) {
		w = w / 2;
		Vector3[] q = new Vector3[4];

		Vector3 n = Vector3.Cross(s, e);
		Vector3 l = Vector3.Cross(n, e-s);
		l.Normalize();
		
		q[0] = transform.InverseTransformPoint(s + l * w);
		q[1] = transform.InverseTransformPoint(s + l * -w);
		q[2] = transform.InverseTransformPoint(e + l * w);
		q[3] = transform.InverseTransformPoint(e + l * -w);

		return q;
	}
	
	void AddLine(Mesh m, Vector3[] quad, bool tmp) {
			int vl = m.vertices.Length;
			
			Vector3[] vs = m.vertices;
			if(!tmp || vl == 0) vs = resizeVertices(vs, 4);
			else vl -= 4;
			
			vs[vl] = quad[0];
			vs[vl+1] = quad[1];
			vs[vl+2] = quad[2];
			vs[vl+3] = quad[3];
			
			int tl = m.triangles.Length;
			
			int[] ts = m.triangles;
			if(!tmp || tl == 0) ts = resizeTraingles(ts, 6);
			else tl -= 6;
			ts[tl] = vl;
			ts[tl+1] = vl+1;
			ts[tl+2] = vl+2;
			ts[tl+3] = vl+1;
			ts[tl+4] = vl+3;
			ts[tl+5] = vl+2;
			
			m.vertices = vs;
			m.triangles = ts;
			m.RecalculateBounds();
	}
	
	Vector3 GetNewPoint() {
        return transform.GetChild(3).position;
	}
	
	Vector3[] resizeVertices(Vector3[] ovs, int ns) {
		Vector3[] nvs = new Vector3[ovs.Length + ns];
		for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
		return nvs;
	}

    int[] resizeTraingles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

}







