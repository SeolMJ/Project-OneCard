using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class CardEditor : Editor
{
    Player card;

    private void Awake()
    {
        card = (Player)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(16);

        if (GUILayout.Button("Start"))
        {
            CardManager.instance?.Resume();
        }

        GUILayout.Space(8);

        if (GUILayout.Button("Quit"))
        {
            CardManager.instance?.Quit(card);
        }
    }
}

[CustomEditor(typeof(ChatElement))]
public class ChatElementEditor : Editor
{
    ChatElement element;

    private void Awake()
    {
        element = (ChatElement)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(16);

        if (GUILayout.Button("Apply"))
        {
            element.Apply();
        }
    }
}

[CustomEditor(typeof(ChatManager))]
public class ChatManagerEditor : Editor
{
    ChatManager manager;

    private void Awake()
    {
        manager = (ChatManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(16);

        if (GUILayout.Button("Layout"))
        {
            if (!ChatManager.instance) ChatManager.instance = manager;
            ChatManager.Layout();
        }
    }
}

[CustomEditor(typeof(NPCInfo)), CanEditMultipleObjects]
public class NPCInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(16);

        if (GUILayout.Button("Randomize Behavior"))
        {
            foreach (Object targetObject in serializedObject.targetObjects)
            {
                (targetObject as NPCInfo).RandomizeBehaviors();
            }
        }
    }
}