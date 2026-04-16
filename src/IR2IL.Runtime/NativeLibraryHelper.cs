using System.Runtime.InteropServices;

namespace IR2IL.Runtime;

public static class NativeLibraryHelper
{
    private static readonly Dictionary<string, nint> LibraryCache = [];

    public static nint GetExport(string libraryName, string exportName)
    {
        var libraryHandle = GetLibrary(libraryName);
        return NativeLibrary.GetExport(libraryHandle, exportName);
    }

    private static nint GetLibrary(string libraryName)
    {
        if (!LibraryCache.TryGetValue(libraryName, out var handle))
        {
            handle = NativeLibrary.Load(libraryName);
            LibraryCache[libraryName] = handle;
        }
        return handle;
    }
}