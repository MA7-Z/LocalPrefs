#if UNITY_WEBGL
#nullable enable

using System;
using System.Runtime.InteropServices;

namespace AndanteTribe.IO.Unity
{
    public static class LSUtils
    {
        public static void WriteAllBytes(in string path, in ReadOnlySpan<byte> bytes) =>
            SaveToLocalStorage(path, Convert.ToBase64String(bytes));

        public static void WriteAllText(in string path, in string contents) =>
            SaveToLocalStorage(path, contents);

        public static void Delete(in string path) => DeleteFromLocalStorage(path);

        public static byte[] ReadAllBytes(in string path)
        {
            var data = LoadFromLocalStorage(path);
            if (string.IsNullOrEmpty(data))
            {
                return Array.Empty<byte>();
            }

            var bytes = Convert.FromBase64String(data);
            return bytes;
        }

        public static string ReadAllText(in string path)
        {
            var data = LoadFromLocalStorage(path);
            return string.IsNullOrEmpty(data) ? "" : data;
        }

        [DllImport("__Internal")]
        private static extern void SaveToLocalStorage(string key, string value);

        [DllImport("__Internal")]
        private static extern void DeleteFromLocalStorage(string key);

        [DllImport("__Internal")]
        private static extern string LoadFromLocalStorage(string key);
    }
}

#endif