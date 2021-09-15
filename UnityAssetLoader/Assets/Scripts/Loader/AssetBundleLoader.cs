using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO:需要解决依赖加载

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
        private Dictionary<string, BundleWrap> _cacheBundleDict = new();
        private Dictionary<string, string[]> _dependenciesPath = new();

        public void LoadDependencies(string mainfest)
        {

        }

        private BundleWrap AddBundleWarp(string abPath, AssetBundle assetBundle)
        {
            BundleWrap bundleWrap = null;
            if (!_cacheBundleDict.TryGetValue(abPath,out bundleWrap))
            {
                bundleWrap = new();
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
            var path = handler.Path;
            var dependencies = QueryDependencies(path);

            foreach(var depPath in dependencies)
            {
                //TODO:回调设置
                var depHandler = Load(depPath);
                depHandler.OnCompoleted += (result) =>
                {
                    //TODO:必须等子Handler回调完才回调自己
                };

            }


            StartCoroutine(OnLoadAsset(handler));
        }

        protected override void OnComplete(LoaderHandler handler)
        {
            //如果
            throw new System.NotImplementedException();
        }

        //加载元操作
        private IEnumerator OnLoadAsset(LoaderHandler handler)
        {
            var abPath = handler.Path;
            AssetBundle assetBundle = null;
            var bundleWrap = GetBundleWarp(abPath);
            if (bundleWrap != null)
            {
                assetBundle = bundleWrap.assetBundle;
                bundleWrap.Retain();
            }
            else
            {
                var abRequest = AssetBundle.LoadFromFileAsync(abPath);
                yield return abRequest;

                bool isDone = abRequest.isDone;
                assetBundle = abRequest.assetBundle;

                if (isDone)
                {
                    //把AB包缓存起来
                    bundleWrap = AddBundleWarp(abPath, assetBundle);
                    bundleWrap.Retain();
                }
            }

            object asset = assetBundle;
            string assetName = "";
            if (!string.IsNullOrEmpty(assetName))
            {
                var assetRequest = assetBundle.LoadAssetAsync(assetName);
                yield return assetRequest;

                asset = assetRequest.asset;

            }
            
            //其他依赖加载完了再返回
            //TODO:这里标记下状态,表示加载结束
            var result = new LoaderResult();
            result.data = asset;
            handler.OnCompoleted(result);

            yield break;
        }

        protected override void OnUnload(LoaderHandler handler)
        {
            //把所有的依赖全部清Release

        }
    }
}

