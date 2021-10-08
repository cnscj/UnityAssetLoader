using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLibrary.Package;

namespace CJGame
{
    public class LoaderManager : MonoSingleton<LoaderManager>
    {

        private readonly Queue<LoaderTask> _prepareTasks = new Queue<LoaderTask>();
        private Dictionary<string, BaseLoader> _loaderMap = new Dictionary<string, BaseLoader>();

        public T GetOrCreateLoader<T>() where T : BaseLoader
        {
            var typeInfo = typeof(T);
            string loaderName = typeInfo.FullName;

            BaseLoader loader = default;
            if (!_loaderMap.TryGetValue(loaderName,out loader))
            {
                var monoGo = new GameObject(loaderName);
                monoGo.transform.SetParent(gameObject.transform);

                loader = monoGo.AddComponent<T>();

                _loaderMap.Add(loaderName, loader);
            }

            return loader as T;
        }


        private void Update()
        {
            
        }

    }
}