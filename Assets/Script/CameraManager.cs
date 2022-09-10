using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SeolMJ;

public class CameraManager : MonoBehaviour
{

    public static CameraManager instance;
    public static Vector2 position, destination;

    [Header("Settings")]
    public Vector3 offset;

    private Vector2 velocity;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Sync();
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void OnUpdate()
    {
        Utils.SmoothDamp(ref position, destination, ref velocity, GameManager.deltaTime);
    }

    void LateUpdate()
    {
        Sync();
    }

    public static void Sync()
    {
        instance.transform.position = (Vector3)position + instance.offset;
    }

}
