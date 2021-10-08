using System;

namespace CJGame
{
    public class LoaderTask
    {
        public readonly LoaderHandler handler;
        public Action<LoaderResult> onSuccess;
        public Action<int> onFailed;

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
        }

        public void OnCallback(LoaderHandler handler)
        {
            handler.loader.CallTaskFinish(this);

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