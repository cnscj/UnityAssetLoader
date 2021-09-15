using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public abstract class BaseLoader : MonoBehaviour
    {
        private readonly Dictionary<string, LoaderHandler> _allHandlers = new();

        private readonly Queue<LoaderHandler> _prepareHandlers = new();
        private readonly HashSet<LoaderHandler> _loadingHandlers = new();
        private readonly Queue<LoaderHandler> _finishedHandlers = new();

        public LoaderHandler Load(string path)
        {
            LoaderHandler handler = null;
            if (!_allHandlers.TryGetValue(path, out handler))
            {
                handler = new LoaderHandler();
                handler.Path = path;
                handler.Loader = this;

                _prepareHandlers.Enqueue(handler);
            }

            handler.Tick();
            return handler;
        }

        public void Unload(LoaderHandler handler)
        {
            OnUnload(handler);
        }

        private void Update()
        {
            DealPrepareHandlers();
            DeadlLoadingHandlers();
            DealFinishedHandlers();
        }

        private void DealPrepareHandlers()
        {
            //负责检查是否有
            while (_prepareHandlers.Count > 0)
            {
                var handler = _prepareHandlers.Dequeue();
                OnLoad(handler);    //进行加载
            }
        }

        private void DeadlLoadingHandlers()
        {
            //进行状态轮询和超时移除
            foreach(var handler in _loadingHandlers)
            {
                if (handler.IsTimeout())
                {
                    _loadingHandlers.Remove(handler);
                }
                else if (handler.IsCompoleted())
                {
                    _finishedHandlers.Enqueue(handler);
                    _loadingHandlers.Remove(handler);
                }
            }
        }

        private void DealFinishedHandlers()
        {
            while (_finishedHandlers.Count > 0)
            {
                var handler = _finishedHandlers.Dequeue();
                OnComplete(handler);
            }
        }

        protected abstract void OnLoad(LoaderHandler handler);
        protected virtual void OnUnload(LoaderHandler handler) { }
        protected abstract void OnComplete(LoaderHandler handler);
    }
}

