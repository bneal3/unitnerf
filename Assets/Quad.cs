using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public Mesh mesh;

    public Quad(Vector3 normal, Vector3 offset)
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
        normals = new Vector3[] { normal, normal, normal, normal };

        // switch(side)
        // {
        //     case NeRFRenderer.BlockSide.FRONT:
        //         {
        //             vertices = new Vector3[] { p4, p5, p1, p0 };
        //             normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        //             break;
        //         }
        //     case NeRFRenderer.BlockSide.BACK:
        //         {
        //             vertices = new Vector3[] { p6, p7, p3, p2 };
        //             normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
        //             break;
        //         }
        //     case NeRFRenderer.BlockSide.TOP:
        //         {
        //             vertices = new Vector3[] { p7, p6, p5, p4 };
        //             normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        //             break;
        //         }
        //     case NeRFRenderer.BlockSide.BOTTOM:
        //         {
        //             vertices = new Vector3[] { p0, p1, p2, p3 };
        //             normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
        //             break;
        //         }
        //     case NeRFRenderer.BlockSide.LEFT:
        //         {
        //             vertices = new Vector3[] { p7, p4, p0, p3 };
        //             normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
        //             break;
        //         }
        //     case NeRFRenderer.BlockSide.RIGHT:
        //         {
        //             vertices = new Vector3[] { p5, p6, p2, p1 };
        //             normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
        //             break;
        //         }
        // }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = new Vector2[] { new Vector2(1.0f, 1.0f),  new Vector2(0.0f, 1.0f), 
                                new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f) };
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }
}
