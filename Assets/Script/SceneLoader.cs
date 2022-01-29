using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader instance;

    [Header("UI")]
    public Image maskImage;

    // Statics
    private static bool loading;
    private static List<int> activeScenes = new();
    private static List<int> sceneQueue = new();
    private static GameObject prefab;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(DoLoad());
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public static void Load(params int[] scenes) => Load(new List<int>(scenes));

    public static void Load(List<int> scenes)
    {
        if (instance == null)
        {
            sceneQueue = new(scenes);
            if (!prefab) prefab = Resources.Load<GameObject>("SceneLoader");
            Instantiate(prefab);
        }
        else
        {
            // Todo
            return;
        }
    }

    public static void LoadLevel(int scene)
    {
        List<int> scenes = new(GameManager.Resource.gameScenes);
        scenes.Add(scene);
        Load(scenes);
    }

    public static void Return()
    {
        SetScene(0);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public static void SetScene(int index)
    {
        activeScenes.Clear();
        activeScenes.Add(index);
    }

    const float fillSpeed = 5f;

    IEnumerator DoLoad()
    {
        loading = true;

        float fill = 0f;
        while (fill != 1f)
        {
            fill = Mathf.MoveTowards(fill, 1f, Time.unscaledDeltaTime * fillSpeed);
            maskImage.fillAmount = Mathf.Sqrt(fill);
            yield return null;
        }

        foreach (int scene in sceneQueue)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            yield return new WaitUntil(() => operation.isDone);
        }

        foreach (int scene in activeScenes)
        {
            if (sceneQueue.Contains(scene))
            {
                sceneQueue.Remove(scene);
                continue;
            }
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        yield return new WaitForSecondsRealtime(0.1f);

        while (fill != 0f)
        {
            fill = Mathf.MoveTowards(fill, 0f, Time.unscaledDeltaTime * fillSpeed);
            maskImage.fillAmount = fill * fill;
            yield return null;
        }

        loading = false;

        Destroy(gameObject);
    }

}
