using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCardRenderer : MonoBehaviour
{

    [Header("Resources")]
    public Mesh mesh;
    public Material material;

    [Header("Settings")]
    public float speed;
    public Vector3 size;
    public AnimationCurve curve;

    [Header("Debug")]
    public int cardCount;

    public static List<MiniCard> miniCards = new(128);
    public Matrix4x4[] matrices = new Matrix4x4[1023];

    void Start()
    {
        miniCards = new(128);
        matrices = new Matrix4x4[1023];
    }

    void Update()
    {
        int count = miniCards.Count;
        cardCount = 0;
        if (count == 0) return;

        Quaternion rotation = Player.instance.cameraTransform.rotation;

        float deltaTime = GameManager.deltaTime;
        float deltaSpeed = speed * deltaTime;
        for (int i = 0; i < count; i++)
        {
            MiniCard card = miniCards[i];
            if (card.progress > 0.999f)
            {
                miniCards.RemoveAt(i);
                i--;
                count--;
                continue;
            }
            if (card.progress < 0f)
            {
                card.progress += deltaTime;
                miniCards[i] = card;
                matrices[i] = Matrix4x4.TRS(card.start, Quaternion.identity, Vector3.zero);
            }
            else
            {
                card.progress = Mathf.Lerp(card.progress, 1f, deltaSpeed);
                float eval = curve.Evaluate(card.progress);
                matrices[i] = Matrix4x4.TRS(Vector3.Lerp(card.start, card.end, card.progress), rotation, new Vector3(size.x * eval, size.y * eval, 2f - eval));
                miniCards[i] = card;
            }
        }
        cardCount = count;
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count);
    }

    public static void Add(Vector3 start, Vector3 end, float progress = 0f)
    {
        if (miniCards.Count > 1022) miniCards.RemoveAt(0);
        miniCards.Add(new(start, end, progress));
    }

}

public struct MiniCard
{
    public float progress;
    public Vector3 start;
    public Vector3 end;

    public MiniCard(Vector3 start, Vector3 end, float progress)
    {
        this.progress = progress;
        this.start = start;
        this.end = end;
    }
}