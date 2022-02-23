using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCard : MonoBehaviour
{

    public float speed;

    public void Animate()
    {
        transform.Rotate(0f, 0f, Time.smoothDeltaTime * speed);
    }

}
