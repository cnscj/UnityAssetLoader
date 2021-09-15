using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public class LoaderHandler
    {
        public LoadResultCallback1 OnCompoleted;

        //public LoadState State{ get; set; }
        public BaseLoader Loader { get; set; }
        public string Path{ get; set; }
        public float Timeout { get; set; }
        public bool IsCancel { get; private set; }

        private float _tickTime;

        public bool IsTimeout()
        {
            if (Timeout > 0)
            {

            }
            return false;
        }

        public bool IsCompoleted()
        {
            return false;
        }

        public void Tick()
        {
            _tickTime = Time.realtimeSinceStartup;
        }

        public void Unload()
        {
            Loader.Unload(this);
        }


    }
}

