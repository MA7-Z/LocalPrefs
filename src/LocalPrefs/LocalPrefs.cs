using System.Reflection;

namespace AndanteTribe.IO;

using System;
using System.Linq;

/// <summary>
/// Provides access to the shared instance of <see cref="ILocalPrefs"/>.
/// </summary>
public static class LocalPrefs
{
    private static ILocalPrefs? s_shared;

    /// <summary>
    /// Shared instance of <see cref="ILocalPrefs"/>.
    /// </summary>
    public static ILocalPrefs Shared
    {
        get => s_shared ??= CreateShared(Assembly.GetCallingAssembly());
        internal set => s_shared = value;
    }

    private static ILocalPrefs CreateShared(Assembly callingAssembly)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type => typeof(ILocalPrefs).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .FirstOrDefault();

        if (type == null)
        {
            throw new InvalidOperationException("No implementation of ILocalPrefs found.");
        }

        var savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            callingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "DefaultCompany",
            callingAssembly.GetName().Name,
            "localprefs-shared");

        try
        {
            return (ILocalPrefs)Activator.CreateInstance(type, savePath, null);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to create an instance of ILocalPrefs implementation: {e.Message}", e);
        }
    }
}