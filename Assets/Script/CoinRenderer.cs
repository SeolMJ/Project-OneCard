using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CoinRenderer : MonoBehaviour
{

    [Header("Resources")]
    public Mesh mesh;
    public Material material;

    [Header("Settings")]
    public float speed;
    public Vector3 size;
    public AnimationCurve moveCurve;
    public AnimationCurve scaleCurve;
    public AnimationCurve alphaCurve;

    [Header("Debug")]
    public int cardCount;

    public static List<Coin> coins = new(128);
    public Matrix4x4[] matrices = new Matrix4x4[1023];

    void Start()
    {
        coins = new(128);
        matrices = new Matrix4x4[1023];
    }

    void Update()
    {
        Camera camera = Camera.main;
        if (!camera) return;

        int count = coins.Count;
        cardCount = 0;
        if (count == 0) return;

        float deltaTime = Time.deltaTime;
        float deltaSpeed = speed * deltaTime;
        Vector2 end = camera.ViewportToWorldPoint(new Vector3(0, 1, 10)) + new Vector3(0.5f, -0.5f, 0f);
        for (int i = 0; i < count; i++)
        {
            Coin card = coins[i];
            if (card.progress > 1f)
            {
                coins.RemoveAt(i);
                i--;
                count--;
                continue;
            }
            
            if (card.progress < 0f)
            {
                card.progress += deltaTime;
                coins[i] = card;
                matrices[i] = Matrix4x4.TRS(card.start, Quaternion.identity, Vector3.zero);
            }
            else
            {
                card.progress += deltaSpeed;
                float eval = scaleCurve.Evaluate(card.progress);
                float moveProgress = moveCurve.Evaluate(card.progress);
                matrices[i] = Matrix4x4.TRS(Vector2.Lerp(card.start, end, moveProgress), Quaternion.identity, new Vector3(size.x * eval, size.y * eval, alphaCurve.Evaluate(card.progress)));
                coins[i] = card;
            }
        }
        cardCount = count;
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count);
    }

    public static void Add(Vector2 start, float progress = 0f)
    {
        if (coins.Count > 1022) coins.RemoveAt(0);
        coins.Add(new(start, progress));
    }

    public static void Add(Vector2 start, int count)
    {
        if (count < 1) return;
        if (coins.Count > 1023 - count)
        {
            coins.RemoveRange(0, count - 1);
        }
        for (int i = 0; i < count; i++)
        {
            coins.Add(new(start, i * -0.1f));
        }
    }

}

public struct Coin
{
    public float progress;
    public Vector2 start;

    public Coin(Vector2 start, float progress)
    {
        this.progress = progress;
        this.start = start;
    }
}