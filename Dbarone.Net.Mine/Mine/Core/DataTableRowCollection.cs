using System.Collections;
using Dbarone.Net.Document;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a collection of rows in a data table.
/// </summary>
public class DataTableRowCollection : IEnumerable<DictionaryDocument>
{
    private DataTable _table;

    public DataTableRowCollection(DataTable table)
    {
        this._table = table;
    }

    IEnumerator<DictionaryDocument> IEnumerable<DictionaryDocument>.GetEnumerator()
    {
        return new DictionaryDocumentEnumerator(_table);
    }

    public IEnumerator GetEnumerator()
    {
        return GetEnumerator();
    }
}