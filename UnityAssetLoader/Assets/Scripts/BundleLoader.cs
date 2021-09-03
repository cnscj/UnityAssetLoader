using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundle加载器
/// </summary>
namespace CJGame
{
    public class BundleLoader : BaseLoader
    {
        protected class BundleRef
        {
            public readonly AssetBundle assetBundle;
            public int RefCount { get; private set; }

            public BundleRef(AssetBundle assetBundle)
            {
                this.assetBundle = assetBundle;
            }

            public void Retain()
            {
                RefCount++;
            }
            public void Release()
            {
                RefCount--;
                if (RefCount <= 0)
                {
                    assetBundle.Unload(true);
                }
            }
        }

        private Dictionary<string, BundleRef> _cacheBundleDict;

    }
}

