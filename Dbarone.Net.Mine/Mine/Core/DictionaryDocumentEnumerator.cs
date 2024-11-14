using System.Collections;
using Dbarone.Net.Document;

namespace Dbarone.Net.Mine;

/// <summary>
/// Provides iterator class for DataTable. Allows foreach syntax over data tables.
/// </summary>
public class DictionaryDocumentEnumerator : IEnumerator<DictionaryDocument>
{
    private DataTable _table;
    private int _i;
    private DictionaryDocument? _current;

    public DictionaryDocumentEnumerator(DataTable table)
    {
        this._table = table;
        this._i = -1;
        this._current = default(DictionaryDocument);
    }

    public DictionaryDocument Current => this._current;

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext()
    {
        if (++this._i >= this._table.Document.Count())
        {
            return false;
        }
        else
        {
            _current = _table.Document[this._i].AsDocument;
            return true;
        }
    }

    public void Reset()
    {
        this._i = -1;
        this._current = default(DictionaryDocument);
    }
}