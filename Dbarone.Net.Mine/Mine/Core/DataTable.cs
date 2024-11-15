using Dbarone.Net.Document;
using Dbarone.Net.Csv;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections;
using Dbarone.Net.Extensions;
using System.Text.Json.Serialization.Metadata;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a table of data. Similar to pandas dataframe
/// </summary>
public class DataTable
{
    SchemaElement _schema;
    DocumentArray _document;
    DataTableRowCollection _rows;

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
        this._rows = new DataTableRowCollection(this);
    }

    public DocumentArray Document
    {
        get { return this._document; }
    }

    public IEnumerable<string> Columns => this._schema.Element.Attributes.Select(a => a.AttributeName);

    public DataColumn Column(string name)
    {
        var items = this._document.Select(r => r.AsDocument[name]).ToList();
        DocumentArray arr = new DocumentArray(items);
        return new DataColumn(arr, name);
    }

    public DataTableRowCollection Rows => _rows;

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

    public static DataTable ReadJson(Stream stream)
    {
        var options = new JsonSerializerOptions();
        options.WriteIndented = true;
        options.Converters.Add(new ObjectToInferredTypesConverter());
        var data = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(stream, options);

        var d = new DocumentValue(data);
        DocumentValue doc = new DocumentArray(data.Select(r => new DocumentValue(r)));
        return new DataTable(doc);
    }

}