using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{



    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void LoadLevel(int index)
    {
        SceneLoader.LoadLevel(index);
    }

}
