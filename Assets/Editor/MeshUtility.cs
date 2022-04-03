using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MeshUtility
{

    /*/
     *  2---3
     *  | \ |
     *  0---1
    /*/

    [MenuItem("Mesh Util/Two Sided Quad")]
    public static void CreateTwoSidedQuad()
    {
        Mesh mesh = new();
        mesh.vertices = new Vector3[8]
        {
            new(-0.5f, -0.5f, 0.0f),
            new(0.5f, -0.5f, 0.0f),
            new(-0.5f, 0.5f, 0.0f),
            new(0.5f, 0.5f, 0.0f),
            new(-0.5f, -0.5f, 0.0f),
            new(0.5f, -0.5f, 0.0f),
            new(-0.5f, 0.5f, 0.0f),
            new(0.5f, 0.5f, 0.0f),
        };
        mesh.triangles = new int[12]
        {
            0, 2, 1,
            2, 3, 1,
            5, 7, 4,
            7, 6, 4,
        };
        mesh.normals = new Vector3[8]
        {
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
        };
        mesh.uv = new Vector2[8]
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),
        };
        AssetDatabase.CreateAsset(mesh, "Assets/TwoSidedQuad.asset");
    }

    [MenuItem("Mesh Util/Two Sided Quad Up")]
    public static void CreateTwoSidedQuadUp()
    {
        Mesh mesh = new();
        mesh.vertices = new Vector3[8]
        {
            new(-0.5f, 0.0f, -0.5f),
            new(0.5f, 0.0f, -0.5f),
            new(-0.5f, 0.0f, 0.5f),
            new(0.5f, 0.0f, 0.5f),
            new(-0.5f, 0.0f, -0.5f),
            new(0.5f, 0.0f, -0.5f),
            new(-0.5f, 0.0f, 0.5f),
            new(0.5f, 0.0f, 0.5f),
        };
        mesh.triangles = new int[12]
        {
            0, 2, 1,
            2, 3, 1,
            5, 7, 4,
            7, 6, 4,
        };
        mesh.normals = new Vector3[8]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.down,
        };
        mesh.uv = new Vector2[8]
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),
        };
        AssetDatabase.CreateAsset(mesh, "Assets/TwoSidedQuadUp.asset");
    }

}
