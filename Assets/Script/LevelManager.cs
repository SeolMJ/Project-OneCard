using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager current;

    void Awake()
    {
        if (GameManager.instance == null)
        {
            int scene = gameObject.scene.buildIndex;
            if (!SceneLoader.Reserved && !SceneLoader.Running) SceneLoader.Reserve(() => SceneLoader.LoadLevel(scene));
            SceneLoader.Return();
            return;
        }
        current = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

}
