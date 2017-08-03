﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AuraFog : MonoBehaviour {
    const string PROP_DIRECTION = "_Direction";
    const string PROP_BLUR_TEX = "_BlurTex";
    const string PROP_TONE = "_Tone";
    const string PROP_COLOR = "_Color";

    const string KW_VERTICAL = "VERTICAL";

    [SerializeField]
    Shader shader;
    [SerializeField]
    Color fogColor;
    [SerializeField]
    [Range(0, 4)]
    int lod = 1;
    [SerializeField]
    [Range(0, 20)]
    int blurIterationCount = 1;
    [SerializeField]
    Vector4 tone = new Vector4 (1f, 1f, 1f, 1f);

    Material mat;
    DepthTextureMode lastDepthTexMode;

    #region Unity
    void OnEnable() {
        mat = new Material (shader);
    }
    void OnPreRender() {
        var c = Camera.current;
        lastDepthTexMode = c.depthTextureMode;
        c.depthTextureMode = DepthTextureMode.Depth;
    }
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        var w = src.width >> lod;
        var h = src.height >> lod;

        var tmp0 = RenderTexture.GetTemporary (w, h, 0, RenderTextureFormat.R8);
        var tmp1 = RenderTexture.GetTemporary (tmp0.width, tmp0.height, 0, tmp0.format);

        Graphics.Blit (src, tmp1, mat, 0);
        Swap (ref tmp0, ref tmp1);

        for (var i = 0; i < 2 * blurIterationCount; i++) {
            if ((i % 2) == 0)
                mat.EnableKeyword (KW_VERTICAL);
            else
                mat.DisableKeyword (KW_VERTICAL);
            Graphics.Blit (tmp0, tmp1, mat, 1);
            Swap (ref tmp0, ref tmp1);
        }

        mat.SetColor (PROP_COLOR, fogColor);
        mat.SetVector (PROP_TONE, tone);
        mat.SetTexture (PROP_BLUR_TEX, tmp0);
        Graphics.Blit (src, dst, mat, 2);

        RenderTexture.ReleaseTemporary (tmp0);
        RenderTexture.ReleaseTemporary (tmp1);
    }
    void OnPostRender() {
        Camera.current.depthTextureMode = lastDepthTexMode;
    }
    void OnDisable() {
        ReleaseObject (mat);
    }
    #endregion

    void Swap<T>(ref T a, ref T b) where T:Object {
        var tmp = a;
        a = b;
        b = tmp;
    }
    void ReleaseObject(Object obj) {
        if (Application.isPlaying)
            Object.Destroy (obj);
        else
            Object.DestroyImmediate (obj);
    }
}