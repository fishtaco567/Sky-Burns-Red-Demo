using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bloom : PostProcessor {

    [Range(0, 10f)]
    public float Intensity = 1f;
    [Range(0.25f, 2f)]
    public float DeltaDownsample = 1f;
    [Range(0.25f, 2f)]
    public float DeltaUpsample = 0.5f;
    [Range(0f, 10f)]
    public float Threshold = 1f;
    [Range(0f, 1f)]
    public float SoftThreshold;
    [Range(1, 8)]
    public int Iterations;

    public Shader bloomShader;

    [NonSerialized]
    private Material bloom;

    private RenderTexture[] tempRTs = new RenderTexture[8];

    public override void Render(RenderTexture source, RenderTexture dest) {
        if(bloom == null) {
            InitializeMaterial();
        }

        bloom.SetFloat("_Intensity", Intensity);
        bloom.SetFloat("_DeltaDownsample", DeltaDownsample);
        bloom.SetFloat("_DeltaUpsample", DeltaUpsample);

        Vector4 filter = new Vector4();
        filter.x = Threshold;
        float knee = Threshold * SoftThreshold;
        filter.y = filter.x - knee;
        filter.z = 2f * knee;
        filter.w = 0.25f / (knee + 0.00001f);
        bloom.SetVector("_Filter", filter);

        int width = 256 / 2;
        int height = 144 / 2;

        RenderTexture currentDestination = tempRTs[0] = RenderTexture.GetTemporary(width, height, 0, source.format);

        Graphics.Blit(source, currentDestination, bloom, 0);

        RenderTexture currentSource = currentDestination;

        int i = 1;
        for(; i < Iterations; i++) {
            width /= 2;
            height /= 2;
            currentDestination = tempRTs[i] = RenderTexture.GetTemporary(width, height, 0, source.format);
            Graphics.Blit(currentSource, currentDestination, bloom, 1);
            currentSource = currentDestination;
        }

        for(i -= 2; i >= 0; i--) {
            currentDestination = tempRTs[i];
            tempRTs[i] = null;
            Graphics.Blit(currentSource, currentDestination, bloom, 2);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = currentDestination;
        }

        bloom.SetTexture("_SourceTex", source);
        Graphics.Blit(currentSource, dest, bloom, 3);
        RenderTexture.ReleaseTemporary(currentSource);
    }

    private void InitializeMaterial() {
        bloom = new Material(bloomShader);

        bloom.SetFloat("_Intensity", Intensity);
        bloom.SetFloat("_DeltaDownsample", DeltaDownsample);
        bloom.SetFloat("_DeltaUpsample", DeltaUpsample);

        Vector4 filter = new Vector4();
        filter.x = Threshold;
        float knee = Threshold * SoftThreshold;
        filter.y = filter.x - knee;
        filter.z = 2f * knee;
        filter.w = 0.25f / (knee + 0.00001f);
        bloom.SetVector("_Filter", filter);
    }

}