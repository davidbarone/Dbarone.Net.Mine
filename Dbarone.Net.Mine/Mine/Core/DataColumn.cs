using Dbarone.Net.Document;
using System.Collections.Generic;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a column in a DataTable. Similar to Pandas series.
/// </summary>
public class DataColumn
{
    IEnumerable<object> _data;

    string Name { get; set; }

    /// <summary>
    /// Creates a new DataTable object from a document
    /// </summary>
    /// <param name=""></param>
    public DataColumn(IEnumerable<object> data, string name)
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

    public DataColumn Unique()
    {
        return new DataColumn(_data.Distinct(), Name);
    }
}