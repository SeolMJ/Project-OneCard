using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "New Dialogue")]
public class Dialogue : ScriptableObject
{

    [Header("Informations")]
    public new string name;

    [Header("Conversations")]
    public Conversation[] conversations;

}
