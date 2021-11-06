using System;

namespace CJGame
{
    public class LoaderTask
    {
        public readonly LoaderHandler handler;
        public Action<LoaderResult> onSuccess;
        public Action<int> onFailed;

        private LoadTaskCompleted _onCompleted;
        private bool _isRelease;

        public LoaderTask(LoaderHandler handler)
        {
            this.handler = handler;
            _isRelease = true;
        }

        public void Stop()
        {
            handler.RemoveCompoleted(OnCallback);
        }

        public void Unload()
        {
            //释放一次
            if (!_isRelease)
            {
                handler.loader.Unload(this);
                _isRelease = true;
            }
        }

        public void Clear()
        {
            Stop();
            Unload();

            onSuccess = null;
            onFailed = null;
            _onCompleted = null;
        }

        public void AddCallback(LoadTaskCompleted callback)
        {
            _onCompleted += callback;
        }

        public void RemoveCallback(LoadTaskCompleted callback)
        {
            _onCompleted -= callback;
        }

        public void OnCallback(LoaderHandler handler)
        {
            _onCompleted?.Invoke(this);

            var result = handler.result;
            if (result.isDone)
            {
                _isRelease = false;
                onSuccess?.Invoke(result);
            }
            else
            {
                onFailed?.Invoke(-1);
            }

        }

    }
}