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
            const string savePath = "localprefs-shared";
#else
            var savePath = UnityEngine.Application.persistentDataPath + "/localprefs-shared";
#endif

#if ENABLE_MESSAGEPACK
            Shared = new MessagePack.MessagePackLocalPrefs(savePath);
#else
            Shared = new Json.JsonLocalPrefs(savePath);
#endif
        }
    }
}