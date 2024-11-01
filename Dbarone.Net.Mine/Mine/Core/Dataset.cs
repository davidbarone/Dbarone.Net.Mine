using Dbarone.Net.Document;
using System.Reflection;

namespace Dbarone.Net.Mine;

/// <summary>
/// Pre-built datasets available for testing in this package.
/// </summary>
public class Dataset
{

    /// <summary>
    /// Gets the string value of a dataset resource.
    /// </summary>
    /// <param name="dataset">The data to get.</param>
    /// <returns>Returns the string contents of the resource.</returns>
    public static string GetString(DatasetEnum dataset)
    {
        var datasetStr = dataset.ToString();
        var assembly = typeof(Dataset).GetTypeInfo().Assembly;
        var path = GetResources().First(r => r.Contains(datasetStr));
        Stream resource = assembly.GetManifestResourceStream(path)!;
        StreamReader sr = new StreamReader(resource);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Gets a list of all the resources available.
    /// </summary>
    /// <returns>A string array of all the dataset names.</returns>
    public static string[] GetResources()
    {
        var assembly = typeof(Dataset).GetTypeInfo().Assembly;
        return assembly.GetManifestResourceNames();
    }
}