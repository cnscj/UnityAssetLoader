using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public class LoaderHandler
    {
        public BaseLoader Loader { get; set; }
        public string Path { get; set; }
        public float Timeout { get; set; } = 30;
        public LoaderResult Result { get; set; }

        private LoadCompletedCallback _onCompleted;
        private HashSet<LoaderHandler> _childrenHandlers;
        private int _callCount;
        private float _tickTime;
        private bool _isFinished;

        public bool IsTimeout()
        {
            if (Timeout > 0)
            {
                if (!_isFinished)
                {
                    var curTime = Time.realtimeSinceStartup;
                    if (curTime >= _tickTime + Timeout)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsCompoleted()
        {
            return _isFinished;
        }

        public void Tick()
        {
            _tickTime = Time.realtimeSinceStartup;
        }

        public void Unload()
        {
            if (_childrenHandlers != null)
            {
                foreach (var handler in _childrenHandlers)
                {
                    handler.Unload();
                }
            }

            Loader.Unload(Path);
        }

        public void Release()
        {
            //TODO:应该以Task为单位
        }

        public void Finish()
        {
            _isFinished = true;
            _callCount++;
            OnTryCall();
        }

        public void AddChild(LoaderHandler handler)
        {
            var children = GetChildrenHandlers();
            handler.AddCompoleted(OnClildCall);
            children.Add(handler);
        }

        public void RemoveChild(LoaderHandler handler)
        {
            if (_childrenHandlers != null)
            {
                handler.RemoveCompoleted(OnClildCall);
                _childrenHandlers.Remove(handler);
            }

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

        public void AddCompoleted(LoadCompletedCallback callback)
        {
            _onCompleted += callback;
        }

        public void RemoveCompoleted(LoadCompletedCallback callback)
        {
            _onCompleted -= callback;
        }

        private void OnTryCall()
        {
            var needCallCount = (_childrenHandlers != null ? _childrenHandlers.Count : 0) + 1;//这里要算上自己
            if (_callCount >= needCallCount)
            {
                _onCompleted(this);
            }
        }

        private void OnClildCall(LoaderHandler handler)
        {
            //这里记录下失败名单和handler,并返回到结果集中
            _callCount++;
            OnTryCall();
        }

        //
        private HashSet<LoaderHandler> GetChildrenHandlers()
        {
            _childrenHandlers = _childrenHandlers ?? new HashSet<LoaderHandler>();
            return _childrenHandlers;
        }
    }
}

