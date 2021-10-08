using System.Collections;
using System.Collections.Generic;
using System.IO;
using CJGame;
using UnityEngine;

public class LoadTest1 : MonoBehaviour
{
    private LoaderTask assetTask;

    void Start()
    {
        var assetBundleLoader = LoaderManager.GetInstance().GetOrCreateLoader<AssetBundleLoader>();
        assetBundleLoader.LoadManifest(Path.Combine(Application.streamingAssetsPath, "StreamingAssets"));

        Debug.Log(Application.streamingAssetsPath);

        var task = assetBundleLoader.Load(Path.Combine(Application.streamingAssetsPath, "prefab|assets/resources/prefab/cube.prefab"));
        task.onSuccess = (result) =>
        {
            var cube = result.data as GameObject;
            Object.Instantiate(cube);
            Debug.LogFormat("{0}", result.data);
            //task.Unload();
        };


        //assetBundleLoader.Load(Path.Combine(Application.streamingAssetsPath, "prefab|assets/resources/prefab/cube.prefab"))
        //.onSuccess = (result) =>
        //{
        //    var cube = result.data as GameObject;
        //    Object.Instantiate(cube);
        //    Debug.LogFormat("{0}", result.data);
        //};
    }
}
