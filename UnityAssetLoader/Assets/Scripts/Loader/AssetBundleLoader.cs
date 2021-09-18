using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// AssetBundle加载器
/// </summary>
namespace CJGame
{
    public partial class AssetBundleLoader : BaseLoader
    {
        public class BundleWrap : ObjectRef
        {
            public AssetBundleLoader loader;
            public string abPath;
            public AssetBundle assetBundle;
            protected override void OnDispose()
            {
                assetBundle.Unload(false);
                loader.RemoveBundleWarp(abPath);
            }
        }

    }

    public partial class AssetBundleLoader : BaseLoader
    {
        private Dictionary<string, BundleWrap> _cacheBundleDict = new Dictionary<string, BundleWrap>();
        private Dictionary<string, string[]> _dependenciesPath = new Dictionary<string, string[]>();
        private string _assetBundleRootPath;

        //依赖文件加载
        public void LoadManifest(string mainfestPath)
        {
            //采用同步加载的方式,不然后面的都取不到依赖
            var mainfestAssetBundle = AssetBundle.LoadFromFile(mainfestPath);
            if (mainfestAssetBundle != null)
            {
                //取mainfest所在目录为根目录
                _assetBundleRootPath = Path.GetDirectoryName(mainfestPath);

                AssetBundleManifest mainfest = mainfestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                _dependenciesPath.Clear();

                foreach (string path in mainfest.GetAllAssetBundles())
                {
                    var fullAbPath = Path.Combine(_assetBundleRootPath, path);

                    string[] dps = mainfest.GetAllDependencies(path);

                    List<string> newDps = new List<string>();
                    foreach (var dpPath in dps)
                    {
   
                        var fullDepPath = Path.Combine(_assetBundleRootPath, dpPath);
                        newDps.Add(fullDepPath);
                    }

                    if (newDps.Count > 0)
                        _dependenciesPath.Add(fullAbPath, newDps.ToArray());
                }


                mainfestAssetBundle.Unload(true);
            }
        }

        private BundleWrap AddBundleWarp(string abPath, AssetBundle assetBundle)
        {
            BundleWrap bundleWrap = null;
            if (!_cacheBundleDict.TryGetValue(abPath,out bundleWrap))
            {
                bundleWrap = new BundleWrap();
                bundleWrap.assetBundle = assetBundle;
                bundleWrap.loader = this;
                bundleWrap.abPath = abPath;

                _cacheBundleDict.Add(abPath, bundleWrap);
            }
            return bundleWrap;
        }

        private void RemoveBundleWarp(string abPath)
        {
            _cacheBundleDict.Remove(abPath);
        }

        private BundleWrap GetBundleWarp(string abPath)
        {
            BundleWrap bundleWrap;
            _cacheBundleDict.TryGetValue(abPath, out bundleWrap);
            return bundleWrap;
        }

        private string[] QueryDependencies(string abPath)
        {
            if (_dependenciesPath.TryGetValue(abPath,out var dependencies))
                return dependencies;
            return default;
        }

        protected override void OnLoad(LoaderHandler handler)
        {
            //先加载其他
            //裁剪出ABPath和AssetPath
            TrySplitePaths(handler.Path, out string abPath, out string _);
            var dependencies = QueryDependencies(abPath);

            if (dependencies != null)
            {
                foreach (var depAbPath in dependencies)
                {
                    var depHandler = GetOrCreateHandler(depAbPath);
                    handler.AddChild(depHandler);
                }
            }

            StartCoroutine(OnLoadAsset(handler));
        }


        protected override void OnUnload(LoaderHandler handler)
        {
            TrySplitePaths(handler.Path, out string abPath, out string assetPath);
            var bundleWrap = GetBundleWarp(abPath);
            if (bundleWrap != null)
            {
                bundleWrap.Release();
            }
        }

        private string[] TrySplitePaths(string oriPath,out string abPath, out string assetPath)
        {
            var paths = oriPath.Split('|');
            abPath = paths.Length >= 1 ? paths[0] : null;
            assetPath = paths.Length >= 2 ? paths[1] : null;

            return paths;
        }

        //加载元操作
        private IEnumerator OnLoadAsset(LoaderHandler handler)
        {
            TrySplitePaths(handler.Path, out string abPath, out string assetPath);
            AssetBundle assetBundle = null;
            bool isDone = false;
            var bundleWrap = GetBundleWarp(abPath);
            if (bundleWrap != null)
            {
                assetBundle = bundleWrap.assetBundle;
                //bundleWrap.Retain();
            }
            else
            {
                var abRequest = AssetBundle.LoadFromFileAsync(abPath);
                yield return abRequest;

                isDone = abRequest.isDone;
                assetBundle = abRequest.assetBundle;

                if (isDone)
                {
                    //把AB包缓存起来
                    bundleWrap = AddBundleWarp(abPath, assetBundle);
                    //bundleWrap.Retain();
                }
            }

            object asset = assetBundle;
            if (!string.IsNullOrEmpty(assetPath))
            {
                //这里是用完就释放了AB,一般是实例化后就释放ab
                var assetRequest = assetBundle.LoadAssetAsync(assetPath);
                yield return assetRequest;

                asset = assetRequest.asset;
                isDone = assetRequest.isDone;

            }
            
            //其他依赖加载完了再返回
            var result = new LoaderResult();
            result.data = asset;
            result.isDone = isDone;
            result.path = handler.Path;

            handler.Result = result;  //把结果缓存起来
            handler.Finish();
        }
    }
}

