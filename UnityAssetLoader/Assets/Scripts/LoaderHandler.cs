using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public class LoaderHandler
    {
        public Action<LoaderResult> onSuccess;
        public Action<LoaderResult> onFailed;

        public BaseLoader BaseLoader{ get; set; }
        public string Path{ get; set; }
        public bool IsCancel { get; private set; }

        public void Star()
        {
            if (!BaseLoader)
                return;

            BaseLoader.LaunchHandler(this);
        }

        public void Stop()
        {
            if (!BaseLoader)
                return;
        }

        public void Unload(bool unloadAllLoadedObjefcts = false)
        {

        }

        public void OnCallback(LoaderResult result)
        {
            if(IsCancel)
            {

                return;
            }


        }
    }
}

