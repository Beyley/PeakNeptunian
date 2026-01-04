using System.IO;
using System.Reflection;

namespace PeakNeptunian;

public static class Helpers
{
    public static string? GetEmbeddedResourceContent(string resourceName)
    {
        var asm = Assembly.GetAssembly(typeof(Helpers));
        using var stream = asm.GetManifestResourceStream($"PeakNeptunian.{resourceName}");
        if (stream == null)
        {
            return null;
        }

        using var source = new StreamReader(stream);
        var fileContent = source.ReadToEnd();
        return fileContent;
    }
}