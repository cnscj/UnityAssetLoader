using System;

namespace CJGame
{
    public class LoaderTask
    {
        public readonly LoaderHandler handler;
        public Action<LoaderResult> onSuccess;
        public Action<int> onFailed;

        public LoaderTask(LoaderHandler handler)
        {
            this.handler = handler;
        }

        public void Unload()
        {
            //释放一次
            handler.Release();
        }

        public void Clear()
        {
           
        }

        public void Callback(LoaderHandler handler)
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

            
        }

    }
}