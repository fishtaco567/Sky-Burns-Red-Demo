using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public abstract class PostProcessor : MonoBehaviour {

    public abstract void Render(RenderTexture source, RenderTexture dest);

}
