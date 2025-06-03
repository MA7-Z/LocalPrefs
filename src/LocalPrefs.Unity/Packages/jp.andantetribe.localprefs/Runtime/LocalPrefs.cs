#nullable enable

namespace AndanteTribe.IO.Unity
{
    public static class LocalPrefs
    {
        /// <summary>
        /// Shared instance of <see cref="ILocalPrefs"/>.
        /// </summary>
        public static readonly ILocalPrefs Shared;

        static LocalPrefs()
        {
#if ENABLE_MESSAGEPACK && UNITY_WEBGL && !UNITY_EDITOR
            Shared = new MessagePack.MessagePackLocalPrefs("localprefs-shared", fileAccessor: new LSAccessor());
#elif ENABLE_MESSAGEPACK
            Shared = new MessagePack.MessagePackLocalPrefs(UnityEngine.Application.persistentDataPath + "/localprefs-shared");
#elif UNITY_WEBGL && !UNITY_EDITOR
            Shared = new Json.JsonLocalPrefs("localprefs-shared", fileAccessor: new LSAccessor());
#else
            Shared = new Json.JsonLocalPrefs(UnityEngine.Application.persistentDataPath + "/localprefs-shared");
#endif
        }
    }
}