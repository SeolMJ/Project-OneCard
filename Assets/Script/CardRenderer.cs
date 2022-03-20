using System.Collections.Generic;
using UnityEngine;

public class CardRenderer : MonoBehaviour
{

    public static CardRenderer instance;

    [Header("Resources")]
    public Mesh mesh;
    public Material material;

    [Header("Settings")]
    public float moveSpeed;
    public float angularSpeed;
    public float rotateSpeed;
    public Vector3 size;

    [Header("Debug")]
    public int cardCount;

    public static List<CardEntity> cards = new(128);
    public static List<CardEntity> activeCards = new(64);
    public static Stack<CardEntity> inactiveCards= new(32);
    public Matrix4x4[] matrices = new Matrix4x4[1023];

    public static int i;
    public static int count;
    public static float deltaTime;
    public static float deltaMoveSpeed;
    public static float deltaAngularSpeed;
    public static float deltaRotateSpeed;
    public static float angle;
    public static Vector2 playerPos;
    public static LayerMask enemyLayer;

    public static System.Random random;

    void Awake()
    {
        instance = this;
        random = new System.Random();
    }

    void Start()
    {
        enemyLayer = Player.instance.enemyLayer;
        cards = new(128);
        matrices = new Matrix4x4[1023];
        //for (int i = 0; i < 100; i++) Add();
    }

    void Update()
    {
        Camera camera = Camera.main;
        if (!camera) return;

        count = cards.Count;
        cardCount = 0;
        playerPos = (Vector2)Player.instance.transform.position + new Vector2(0, 0.735f);
        if (count == 0) return;

        deltaTime = GameManager.deltaTime;
        deltaMoveSpeed = moveSpeed * deltaTime;
        deltaAngularSpeed = angularSpeed * deltaTime;
        deltaRotateSpeed = rotateSpeed * deltaTime;
        angle = Mathf.PI * 2f / count;

        for (i = 0; i < count; i++)
        {
            CardEntity card = cards[i];

            if (card.state())
            {
                i--;
                count--;
                continue;
            }

            matrices[i] = Matrix4x4.TRS(card.position, card.rotation, size);
            cards[i] = card;
        }
        cardCount = count;
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count);
    }

    public static void Remove(CardEntity card)
    {
        cards.Remove(card);
        inactiveCards.Push(card);
    }

    public static void Shoot(Transform target, Vector2 direction)
    {
        if (cards.Count > 1022) cards.RemoveAt(0);
        CardEntity card = inactiveCards.Count == 0 ? new() : inactiveCards.Pop();
        card.Init(playerPos, direction, target);
        cards.Add(card);
    }

}

#region Trash
/*
public class FlowCard
{
    public Vector2 position;
    public Quaternion rotation;
    public float scale;

    public delegate bool State();
    public State state;

    public void Init()
    {
        position = (Vector2)Player.instance.transform.position + new Vector2(0, 0.25f);
        rotation = Quaternion.Euler(0, 0, 90f);
        scale = 1.0f;
        CardRenderer.activeCards.Add(this);
        state = NormalState;
        lifeTime = 0f;
    }

    bool NormalState()
    {
        float thisAngle = CardRenderer.angle * CardRenderer.i + CardRenderer.offset;
        position = Vector2.Lerp(position, CardRenderer.playerPos + new Vector2(Mathf.Cos(thisAngle), Mathf.Sin(thisAngle)), CardRenderer.deltaOrbitSmooth);
        return false;
    }

    private Transform target;
    private float lifeTime;

    public void Shoot(Transform target, Vector2 direction)
    {
        this.target = target;
        rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        CardRenderer.activeCards.Remove(this);
        state = ShootState;
    }

    bool ShootState()
    {
        if (!target)
        {
            CardRenderer.activeCards.Add(this);
            state = NormalState;
            return false;
        }

        lifeTime += CardRenderer.deltaTime;

        Vector2 diff = ((Vector2)target.position - position).normalized;
        Vector2 prevPos = position;
        float deltaAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        rotation = Quaternion.Lerp(rotation, Quaternion.Euler(0, 0, deltaAngle), CardRenderer.deltaAngularSpeed + lifeTime * CardRenderer.deltaTime * 50f);
        position += (Vector2)(rotation * Vector3.right) * CardRenderer.deltaMoveSpeed;

        RaycastHit2D hit = Physics2D.Linecast(prevPos, position, CardRenderer.enemyLayer);
        if (hit.collider)
        {
            Init();
            return false;
        }

        return false;
    }

    public void Final()
    {
        CardRenderer.activeCards.Remove(this);
        CardRenderer.Remove(this);
        CardRenderer.i--;
        CardRenderer.Add();
    }

}
*/
#endregion

[System.Serializable]
public class CardEntity
{
    // Transform
    public Vector2 position;
    public Vector2 velocity;
    public Quaternion rotation;

    // State
    public delegate bool State();
    public State state;

    // Target
    public Transform target;

    public void Init(Vector2 position, Vector2 velocity, Transform target)
    {
        this.position = position;
        this.target = target;
        this.velocity = velocity;
        rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        state = Shoot;
    }

    public bool Shoot()
    {
        Vector2 offset = ((Vector2)target.position - position).normalized;
        velocity = Vector2.MoveTowards(velocity, offset, CardRenderer.deltaAngularSpeed);
        Vector2 prevPos = position;
        position += velocity * CardRenderer.deltaMoveSpeed;
        rotation *= Quaternion.Euler(0, 0, CardRenderer.deltaRotateSpeed);
        RaycastHit2D hit = Physics2D.Linecast(prevPos, position, CardRenderer.enemyLayer);
        if (hit.collider)
        {
            state = Return;
            velocity *= 0.5f;
        }
        return false;
    }

    public bool Return()
    {
        Vector2 offset = CardRenderer.playerPos - position;
        velocity = Vector2.MoveTowards(velocity, offset.normalized, CardRenderer.deltaAngularSpeed);
        position += velocity * CardRenderer.deltaMoveSpeed;
        rotation *= Quaternion.Euler(0, 0, CardRenderer.deltaRotateSpeed);
        if (offset.magnitude < 0.5f)
        {
            Final();
            return true;
        }
        return false;
    }

    public void Final()
    {
        CardRenderer.inactiveCards.Push(this);
        CardRenderer.cards.Remove(this);
    }
}