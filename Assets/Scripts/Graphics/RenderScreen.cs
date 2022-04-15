using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderScreen : MonoBehaviour {

    [SerializeField]
    private RenderTexture texture = default;

    public Material mat = default;

    private Camera cam;

    [SerializeField]
    protected PostProcessor[] post;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
	}

    // Update is called once per frame
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if(mat != null) {
            var curDst = RenderTexture.GetTemporary(480, 270);
            curDst.filterMode = FilterMode.Point;
            var curSrc = src;

            for(int i = 0; i < post.Length; i++) {
                var pp = post[i];

                pp.Render(curSrc, curDst);

                if(i != 0) {
                    RenderTexture.ReleaseTemporary(curSrc);
                }

                curSrc = curDst;
                curDst = RenderTexture.GetTemporary(480, 270);
                curDst.filterMode = FilterMode.Point;
            }

            Graphics.Blit(curSrc, curDst, mat);
            RenderTexture.ReleaseTemporary(curSrc);
            curSrc = curDst;
            Graphics.Blit(curSrc, dst);

            RenderTexture.ReleaseTemporary(curSrc);
        } else {
            Graphics.Blit(texture, dst);
        }
	}
}
