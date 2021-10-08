using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLibrary.Package;

namespace CJGame
{
    public abstract class BaseLoader : MonoBehaviour
    {
        private readonly Dictionary<string, LoaderHandler> _allHandlers = new Dictionary<string, LoaderHandler>();

        private readonly Queue<LoaderTask> _prepareTasks = new Queue<LoaderTask>();

        private readonly Queue<LoaderHandler> _prepareHandlers = new Queue<LoaderHandler>();
        private readonly HashSet<LoaderHandler> _loadingHandlers = new HashSet<LoaderHandler>();
        private readonly Queue<LoaderHandler> _finishedHandlers = new Queue<LoaderHandler>();

        public LoaderTask Load(string path)
        {
            var handler = GetOrCreateHandler(path);
            var task = new LoaderTask(handler);

            _prepareTasks.Enqueue(task);

            return task;
        }

        public void Unload(LoaderTask task)
        {
            OnUnload(task.handler);
        }

        public void CallTaskFinish(LoaderTask task)
        {
            OnTaskFinish(task);
        }

        private void Update()
        {
            DealPrepare();
            DealLoading();
            DealFinished();
        }

        private void DealPrepare()
        {
            while (_prepareTasks.Count > 0)
            {
                var task = _prepareTasks.Dequeue();
                var handler = task.handler;

                handler.AddCompoleted(task.OnCallback);
            }

            //负责检查是否有
            while (_prepareHandlers.Count > 0)
            {
                var handler = _prepareHandlers.Dequeue();

                handler.RefreshTick();
                OnLoad(handler);    //进行加载
            }
        }

        private void DealLoading()
        {
            //进行状态轮询和超时检测
            foreach(var handler in _loadingHandlers)
            {
                if (handler.IsTimeout())
                {
                    handler.Finish();   //强制结束
                }

                if (handler.IsCompleted())
                {
                    _finishedHandlers.Enqueue(handler);
                    _loadingHandlers.Remove(handler);
                }
            }
        }

        private void DealFinished()
        {
            //进行收尾工作
            while (_finishedHandlers.Count > 0)
            {
                var handler = _finishedHandlers.Dequeue();
                OnHandlerFinish(handler);

                _allHandlers.Remove(handler.path);
            }
        }

        protected LoaderHandler GetOrCreateHandler(string path)
        {
            if (!_allHandlers.TryGetValue(path, out var handler))
            {
                handler = new LoaderHandler(this,path);
                _allHandlers.Add(path,handler);

                _prepareHandlers.Enqueue(handler);
            }
            return handler;
        }

        protected abstract void OnLoad(LoaderHandler handler);
        protected virtual void OnUnload(LoaderHandler handler) { }

        protected virtual void OnTaskFinish(LoaderTask task) { }
        protected virtual void OnHandlerFinish(LoaderHandler handler) { }
    }
}

