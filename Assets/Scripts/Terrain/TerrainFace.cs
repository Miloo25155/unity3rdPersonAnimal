﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    ShapeGenerator shapeGenerator;

    Mesh mesh;
    MeshCollider meshCollider;

    int resolution;
    Vector3 localUp;

    Vector3 axisA;
    Vector3 axisB;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh(bool useFlatShading, ColorGenerator colorGenerator)
    {
        triangles = new int[(resolution - 1) * (resolution - 1) * 2 * 3];
        vertices = new Vector3[resolution * resolution];
        uvs = new Vector2[resolution * resolution];

        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);

                if(x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;

                    triIndex += 6;
                }
            }
        }

        mesh.Clear();

        if (useFlatShading)
        {
            UpdateUVs(colorGenerator);
            ConstructFlatMesh();
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    public void ConstructFlatMesh()
    {
        Vector3[] flatVertices = new Vector3[triangles.Length];
        Vector2[] flatUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatVertices[i] = vertices[triangles[i]];
            flatUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatVertices;
        uvs = flatUvs;
    }

    public void UpdateUVs(ColorGenerator colorGenerator)
    {
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                uvs[i] = new Vector2(colorGenerator.BiomePercentFromPoint(pointOnUnitSphere), 0);
            }
        }
    }

    public MeshCollider InitMeshCollider(GameObject gameObject)
    {
        if (gameObject.GetComponent<MeshCollider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        return meshCollider;
    }
}
