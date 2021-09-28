using System.Collections;
using System.Collections.Generic;
using System.IO;
using CJGame;
using UnityEngine;

public class LoadTest1 : MonoBehaviour
{
    public AssetBundleLoader assetBundleLoader;
 
    void Start()
    {
        assetBundleLoader = gameObject.GetComponent <AssetBundleLoader>() ?? gameObject.AddComponent<AssetBundleLoader>();
        assetBundleLoader.LoadManifest(Path.Combine(Application.streamingAssetsPath, "StreamingAssets"));

        var task = assetBundleLoader.Load(Path.Combine(Application.streamingAssetsPath, "prefab|assets/resources/prefab/cube.prefab"));
        task.onSuccess = (result) =>
        {
            var cube = result.data as GameObject;
            Object.Instantiate(cube);
            Debug.LogFormat("{0}", result.data);
        };


        assetBundleLoader.Load(Path.Combine(Application.streamingAssetsPath, "prefab|assets/resources/prefab/cube.prefab"))
        .onSuccess = (result) =>
        {
            var cube = result.data as GameObject;
            Object.Instantiate(cube);
            Debug.LogFormat("{0}", result.data);
        };
    }
}
