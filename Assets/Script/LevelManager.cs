using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public Dictionary<Vector2Int, Carder> tiles;

    void Awake()
    {
        if (GameManager.instance == null)
        {
            int scene = gameObject.scene.buildIndex;
            if (!SceneLoader.Reserved && !SceneLoader.Running) SceneLoader.Reserve(() => SceneLoader.LoadLevel(scene));
            SceneLoader.Return();
            return;
        }
        instance = this;
    }

    void Start()
    {
        tiles = new Dictionary<Vector2Int, Carder>();
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                tiles.Add(new(x, y), null);
            }
        }
        CameraManager.position = Vector2.zero;
    }

    void Update()
    {
        
    }

    public static bool Available(Vector2Int position, out Carder carder)
    {
        return instance.tiles.TryGetValue(position, out carder) && !carder;
    }

}
