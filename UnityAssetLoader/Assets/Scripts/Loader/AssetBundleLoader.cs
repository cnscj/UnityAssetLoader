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
                assetBundle.Unload(true);
                loader.RemoveBundleWarp(abPath);
            }
        }

    }

    public partial class AssetBundleLoader : BaseLoader
    {
        private readonly Dictionary<string, BundleWrap> _cacheBundleDict = new Dictionary<string, BundleWrap>();
        private readonly Dictionary<string, string[]> _dependenciesPath = new Dictionary<string, string[]>();

        private string _assetBundleRootPath;

        //依赖文件加载
        public void LoadManifest(string manifestPath)
        {
            //采用同步加载的方式,不然后面的都取不到依赖
            var manifestAssetBundle = AssetBundle.LoadFromFile(manifestPath);
            if (manifestAssetBundle != null)
            {
                //取manifest所在目录为根目录
                _assetBundleRootPath = Path.GetDirectoryName(manifestPath);

                AssetBundleManifest manifest = manifestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                LoadManifest(manifest);

                manifestAssetBundle.Unload(true);
            }
        }

        public void LoadManifest(AssetBundleManifest manifest)
        {
            _dependenciesPath.Clear();

            if (manifest != null)
            {
                foreach (string path in manifest.GetAllAssetBundles())
                {
                    var fullAbPath = Path.Combine(_assetBundleRootPath, path);

                    string[] dps = manifest.GetAllDependencies(path);

                    var newDps = new List<string>();
                    foreach (var dpPath in dps)
                    {
                        var fullDepPath = Path.Combine(_assetBundleRootPath, dpPath);
                        newDps.Add(fullDepPath);
                    }

                    if (newDps.Count > 0)
                        _dependenciesPath.Add(fullAbPath, newDps.ToArray());
                }
            }
        }

        private string[] QueryDependencies(string abPath)
        {
            if (_dependenciesPath.TryGetValue(abPath, out var dependencies))
                return dependencies;
            return default;
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

        protected override void OnLoad(LoaderHandler handler)
        {
            if (handler == null)
                return;

            TrySplitePaths(handler.path, out string abPath, out string _);
            var dependencies = QueryDependencies(abPath);

            if (dependencies != null)
            {
                foreach (var depAbPath in dependencies)
                {
                    if (string.Compare(depAbPath, abPath) == 0)
                        continue;
                        
                    var depHandler = GetOrCreateHandler(depAbPath);
                    handler.AddChild(depHandler);
                }
            }

            StartCoroutine(OnLoadAsset(handler));
        }


        protected override void OnUnload(LoaderHandler handler)
        {
            //释放所有的依赖,递归释放依赖
            if (handler == null)
                return;

            TrySplitePaths(handler.path, out string abPath, out string _);
            var bundleWrap = GetBundleWarp(abPath);
            bundleWrap?.Release();

            var depHandlers = handler.GetChildren();
            if (depHandlers != null)
            {
                foreach (var depHandler in depHandlers)
                {
                    OnUnload(depHandler);
                }
            }
        }

        protected override void OnTaskFinish(LoaderTask task)
        {
            //增加所有handler的引用
            var handler = task.handler;
            if (handler.result.isDone)
            {
                //因为这种直接使用的资源是用完就释放的,所以不需要Retain
                RetainHandlerWithBundle(handler);
            }
        }

        private void RetainHandlerWithBundle(LoaderHandler handler)
        {
            if (handler == null)
                return;

            TrySplitePaths(handler.path, out string abPath, out string _);
            var bundleWrap = GetBundleWarp(abPath);
            bundleWrap?.Retain();

            var depHandlers = handler.GetChildren();
            if (depHandlers != null)
            {
                foreach (var depHandler in depHandlers)
                {
                    RetainHandlerWithBundle(depHandler);
                }
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
            TrySplitePaths(handler.path, out string abPath, out string assetPath);
            AssetBundle assetBundle = null;
            bool isDone = false;
            var bundleWrap = GetBundleWarp(abPath);
            if (bundleWrap != null)
            {
                assetBundle = bundleWrap.assetBundle;
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
            result.path = handler.path;

            handler.result = result;  //把结果缓存起来
            handler.Finish();
        }
    }
}

