﻿using UnityEngine;

public enum HexEdgeType { Flat, Slope, Cliff }

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f; //Mathf.Sqrt(3) / 2;
    public const float elevationStep = 2.5f;
    public static Texture2D noiseSource;
    
    static Vector3[] corners = { new Vector3(0f, 0f, outerRadius),
                                        new Vector3(innerRadius, 0f, 0.5f*outerRadius),
                                        new Vector3(innerRadius, 0f, -0.5f*outerRadius),
                                        new Vector3(0f, 0f, -outerRadius),
                                        new Vector3(-innerRadius, 0f, -0.5f*outerRadius),
                                        new Vector3(-innerRadius, 0f, 0.5f*outerRadius),
                                        new Vector3(0f, 0f, outerRadius)};

    //cell metrics
    public const float solidFactor = .8f;
    public const float blendFactor = 1f - solidFactor;

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner (HexDirection direction) {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction) {
        return corners[(int)direction + 1] * solidFactor;
    }

    //connection metrics
    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    public static Vector3 GetBridge (HexDirection direction) {
        return (corners[(int)direction] + corners[(int)direction + 1])
                * blendFactor;
    }

    public static HexEdgeType GetEdgeType(int elevation1, int elevation2) {
        if (elevation1 == elevation2) { return HexEdgeType.Flat; }
        int delta = elevation1 - elevation2;
        if (delta == 1 || delta == -1) { return HexEdgeType.Slope; }
        return HexEdgeType.Cliff;
    }

    public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;
        return a;
    }

    public static Color TerraceLerp (Color a, Color b, int step) {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    //Noise Metrics
    public const float cellPerturbStrength = 0f; // 4f;
    public const float elevationPerturbStrength = 1.25f;
    public const float noiseScale = .003f;

    public static Vector4 SampleNoise (Vector3 pos) {
        return noiseSource.GetPixelBilinear(pos.x * noiseScale, 
                                            pos.z * noiseScale);
    }

    //Chunk Metrics
    public const int chunkSizeX = 5, chunkSizeZ = 5;

    //river metrics
    public const float streamBedElevationOffset = -1f;
}