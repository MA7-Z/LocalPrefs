#if UNITY_WEBGL && !UNITY_EDITOR
#nullable enable

using UnityEngine;

[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace AndanteTribe.IO.Unity
{
    public static class WebGLFileAccessorInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void SetDefaultFileAccessor() => IFileAccessor.Default = new LSAccessor();
    }
}

#endif