using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SeolMJ;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader instance;

    [Header("UI")]
    public Image maskImage;

    // Statics
    public static bool Loading { get; private set; }
    public static bool Running { get; private set; }
    private static List<int> activeScenes = new();
    private static List<int> sceneQueue = new();
    private static List<Action> reservedScene = new();
    private static GameObject prefab;

    public static bool Reserved => reservedScene.Count > 0;

    void Awake()
    {
        if (Loading) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        StartCoroutine(DoLoad());
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public static void Load(params int[] scenes)
    {
        if (Loading)
        {
            if (!Reserved) Reserve(() => Load(new List<int>(scenes)));
        }
        else Load(new List<int>(scenes));
    }

    public static void Load(List<int> scenes)
    {
        if (instance == null)
        {
            Running = true;
            sceneQueue = scenes;
            if (!prefab) prefab = Resources.Load<GameObject>("SceneLoader");
            Instantiate(prefab);
        }
        else
        {
            // Todo
            return;
        }
    }

    public async static void LoadLevel(int scene)
    {
        while (GameManager.instance == null) await System.Threading.Tasks.Task.Delay((int)(Time.unscaledDeltaTime * 1000f));
        List<int> scenes = new(GameManager.Resource.gameScenes);
        scenes.Add(scene);
        if (Loading)
        {
            if (!Reserved) Reserve(() => Load(scenes));
        }
        else Load(scenes);
    }

    public static void Return()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
        SetScene(0);

        if (reservedScene.Count > 0)
        {
            reservedScene[0].Invoke();
            reservedScene.RemoveAt(0);
            Log("Chain Loading");
        }
    }

    public static void Reserve(Action action)
    {
        reservedScene.Add(action);
    }

    public static void SetScene(int index)
    {
        activeScenes.Clear();
        activeScenes.Add(index);
    }

    public static int GetScene()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    const float fillSpeed = 5f;

    IEnumerator DoLoad()
    {
        Running = true;
        Loading = true;

        Log($"Loading {sceneQueue.Count} Scenes ({activeScenes.Count} Active)", 1);

        GameManager.timeScale = 0;

        float fill = 0f;
        while (fill != 1f)
        {
            fill = Mathf.MoveTowards(fill, 1f, Time.unscaledDeltaTime * fillSpeed);
            maskImage.fillAmount = Mathf.Sqrt(fill);
            yield return null;
        }

        using var _ = new Busy(5);

        for (int i = 0; i < activeScenes.Count; i++)
        {
            if (!sceneQueue.Contains(activeScenes[i])) continue;

            sceneQueue.Remove(activeScenes[i]);
            activeScenes.RemoveAt(i);
            i--;
        }

        sceneQueue.Distinct();

        int[] oldActiveScenes = activeScenes.ToArray();

        foreach (int scene in sceneQueue)
        {
            Log($"Loading Scene {scene}", 2);
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            activeScenes.Add(scene);
            yield return new WaitUntil(() => operation.isDone);
        }

        foreach (int scene in oldActiveScenes)
        {
            Log($"Unloading Scene {scene}", 2);
            AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
            activeScenes.Remove(scene);
            yield return new WaitUntil(() => operation.isDone);
        }

        yield return new WaitForSecondsRealtime(0.1f);

        while (fill != 0f)
        {
            fill = Mathf.MoveTowards(fill, 0f, Time.unscaledDeltaTime * fillSpeed);
            maskImage.fillAmount = fill * fill;
            yield return null;
        }

        Loading = false;
        Running = false;

        GameManager.timeScale = 1f;

        Log("Scene Loaded", 3);

        if (reservedScene.Count > 0)
        {
            reservedScene[0].Invoke();
            reservedScene.RemoveAt(0);
            Log("Chain Loading");
        }

        Destroy(gameObject);
    }

    #region Logging

    public static LogPreset? logPreset;

    public static void Log(string content, byte state = 0)
    {
        logPreset ??= new("Scene", GameManager.instance ? GameManager.Resource.sceneLogColor : new Color32(167, 251, 255, 255));
        Log4u.Log(logPreset.Value, content, state);
    }

    #endregion

}
