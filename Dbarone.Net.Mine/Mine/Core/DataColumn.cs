using Dbarone.Net.Document;
using System.Collections;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a column in a DataTable. Similar to Pandas series.
/// </summary>
public class DataColumn
{
    SchemaElement Schema;
    DocumentValue Document;

    string Name { get; set; }

    /// <summary>
    /// Creates a new DataTable object from a document
    /// </summary>
    /// <param name=""></param>
    public DataColumn(IEnumerable data, string name)
    {
    }

    /// <summary>
    /// Returns the number 
    /// </summary>
    /// <returns></returns>
    public int Count()
    {
        return 0;
    }

    public DataColumn Distinct()
    {
        return null;
    }
}