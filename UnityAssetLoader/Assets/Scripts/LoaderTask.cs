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
            Unload();
            Stop();
            onSuccess = null;
            onFailed = null;
        }

        public void OnCallback(LoaderHandler handler)
        {
            var result = handler.Result;
            if (result.isDone)
            {
                onSuccess?.Invoke(result);
            }
            else
            {
                onFailed?.Invoke(-1);
            }

            handler.loader.CallTaskFinish(this);
        }

    }
}