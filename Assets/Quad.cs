using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public Mesh mesh;

    public Quad(Vector3 normal, Vector3 offset, Camera cam)
    {
        mesh = new Mesh();
        mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[5];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];
        triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0f) + offset;
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0f) + offset;
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset;
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset;
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0f) + offset;
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0f) + offset;
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset;
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset;

        vertices = new Vector3[] { p4, p5, p1, p0 };
        // float z = 3.0f;
        // vertices = new Vector3[] { cam.ViewportToWorldPoint(new Vector3(0.25f, 0.25f, z)), cam.ViewportToWorldPoint(new Vector3(0.75f, 0.25f, z)), cam.ViewportToWorldPoint(new Vector3(0.75f, 0.75f, z)), cam.ViewportToWorldPoint(new Vector3(0.25f, 0.75f, z)) };
        normals = new Vector3[] { normal, normal, normal, normal };

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = new Vector2[] { new Vector2(1.0f, 1.0f),  new Vector2(0.0f, 1.0f), 
                                new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f) };
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }
}
