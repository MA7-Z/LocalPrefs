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
#if UNITY_WEBGL && !UNITY_EDITOR
            var fileAccessor = new LSAccessor("localprefs-shared");
#else
            var fileAccessor = FileAccessor.Create(UnityEngine.Application.persistentDataPath + "/localprefs-shared");
#endif

#if ENABLE_MESSAGEPACK
            Shared = new MessagePack.MessagePackLocalPrefs(fileAccessor);
#else
            Shared = new Json.JsonLocalPrefs(fileAccessor);
#endif
        }
    }
}