using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public class LoaderTask
    {
        public Action<LoaderResult> onSuccess;
        public Action<LoaderResult> onFailed;

        public BaseLoader BaseLoader { get; set; }
        public string Path { get; set; }
        public bool IsCancel { get; private set; }

        private Dictionary<string, LoaderHandler> _taskHandlers;

        public void Star()
        {
            if (!BaseLoader)
                return;
        }

        public void Stop()
        {
            if (!BaseLoader)
                return;
        }

        public void Unload(bool unloadAllLoadedObjefcts = false)
        {

        }
    }
}

