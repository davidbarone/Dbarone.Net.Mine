using Dbarone.Net.Document;
using Dbarone.Net.Csv;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a table of data. Similar to pandas dataframe
/// </summary>
public class DataTable
{
    SchemaElement _schema;
    DocumentArray _document;

    /// <summary>
    /// Creates a new DataTable object from a document
    /// </summary>
    /// <param name=""></param>
    public DataTable(DocumentValue document)
    {
        SchemaElement element = SchemaElement.Parse(document);
        if (!element.IsTabularSchema())
        {
            throw new Exception("Document is not a tabular schema.");
        }
        else if (element.DocumentType != DocumentType.Array)
        {
            throw new Exception("Document must be an array.");
        }
        this._schema = element;
        this._document = document.AsArray;
    }

    public DocumentArray Document
    {
        get { return this._document; }
    }

    public IEnumerable<string> Columns()
    {
        return this._schema.Element.Attributes.Select(a => a.AttributeName);
    }

    public DataColumn Column(string name)
    {
        return null;
    }

    /// <summary>
    /// Creates a new DataTable object from a csv stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static DataTable ReadCsv(Stream stream, CsvConfiguration configuration)
    {
        CsvReader reader = new CsvReader(stream, configuration);
        var data = reader.Read();
        DocumentValue doc = new DocumentArray(data.Select(r => new DocumentValue(r)));
        return new DataTable(doc);
    }

    public static DataTable ReadJson(Stream stream, JsonSerializerOptions configuration)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object>>(stream, configuration);
        DocumentValue doc = new DocumentArray(data.Select(r => new DocumentValue(r)));
        return new DataTable(doc);
    }
}