using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public class LoaderHandler
    {
        public readonly BaseLoader loader;
        public readonly string path;
        public float timeout;
        public LoaderResult result;

        private LoadHandlerCompleted _onCompleted;
        private HashSet<LoaderHandler> _childrenHandlers;
        private int _callCount;
        private float _tickTime;
        private float _doneTime;

        public LoaderHandler(BaseLoader loader,string path)
        {
            this.loader = loader;
            this.path = path;
            this.timeout = 30f;
        }

        public bool IsTimeout()
        {
            if (timeout > 0f)
            {
                var checkTime = IsCompleted() ? _doneTime : Time.realtimeSinceStartup;
                if (checkTime >= _tickTime + timeout)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCompleted()
        {
            return _doneTime > 0f;
        }

        public void Finish()
        {
            if (IsCompleted())
                return;
            
            _callCount++;

            OnTryCall();
        }

        public void AddChild(LoaderHandler handler)
        {
            var children = GetChildrenHandlers();
            if (!children.Contains(handler))
            {
                handler.AddCompoleted(OnClildCall);
                children.Add(handler);
            }
        }

        public void RemoveChild(LoaderHandler handler)
        {
            if (_childrenHandlers != null)
            {
                handler.RemoveCompoleted(OnClildCall);
                _childrenHandlers.Remove(handler);
            }

        }

        public HashSet<LoaderHandler> GetChildren()
        {
            return _childrenHandlers;
        }

        public void RemoveAllChildren()
        {
            if (_childrenHandlers != null)
            {
                foreach(var handler in _childrenHandlers)
                {
                    RemoveChild(handler);
                }
            }
        }

        public void AddCompoleted(LoadHandlerCompleted callback)
        {
            _onCompleted += callback;
        }

        public void RemoveCompoleted(LoadHandlerCompleted callback)
        {
            _onCompleted -= callback;
        }

        public void RefreshTick()
        {
            _tickTime = Time.realtimeSinceStartup;
        }

        private void OnTryCall()
        {
            var needCallCount = (_childrenHandlers != null ? _childrenHandlers.Count : 0) + 1;//这里要算上自己
            if (_callCount >= needCallCount)
            {
                _onCompleted(this);
                _doneTime = Time.realtimeSinceStartup;  //NOTE:如果加载依赖过多,可能会造成等待
            }
        }

        private void OnClildCall(LoaderHandler handler)
        {
            //这里记录下失败名单和handler,并返回到结果集中

            _callCount++;
            OnTryCall();
        }

        private HashSet<LoaderHandler> GetChildrenHandlers()
        {
            _childrenHandlers = _childrenHandlers ?? new HashSet<LoaderHandler>();
            return _childrenHandlers;
        }
    }
}

