using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public abstract class BaseLoader : MonoBehaviour
    {
        private Queue<LoaderHandler> _readyHandlers = new Queue<LoaderHandler>();

        public virtual LoaderHandler Load(string path)
        {
            var handler = new LoaderHandler();
            handler.BaseLoader = this;
            handler.Path = path;

            _readyHandlers.Enqueue(handler);
            return handler;
        }

        public void LaunchHandler(LoaderHandler handler)
        {
            OnLaunch(handler);
        }

        public virtual void Clear()
        {

        }

        private void Update()
        {
            DealReadyHandlers();
        }

        private void DealReadyHandlers()
        {
            while (_readyHandlers.Count > 0)
            {
                var handler = _readyHandlers.Dequeue();
                if (!handler.IsCancel)
                {
                    handler.Star();
                }
            }
        }

        protected virtual void OnLaunch(LoaderHandler handler)
        {

        }

        protected virtual void OnHandlerCallback(LoaderHandler handler)
        {

        }
    }
}

