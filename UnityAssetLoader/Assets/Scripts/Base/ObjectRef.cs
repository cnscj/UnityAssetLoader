using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJGame
{
    public abstract class ObjectRef 
    {
        public int RefCount { get; protected set; }
        public void Retain()
        {
            RefCount++;
        }

        public void Release()
        {
            RefCount--;
            if (RefCount == 0)
            {
                OnDispose();
            }
        }

        protected virtual void OnDispose(){}
    }
}

