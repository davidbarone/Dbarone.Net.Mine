using Dbarone.Net.Document;

namespace Dbarone.Net.Mine;

/// <summary>
/// Represents a table of data. Similar to pandas dataframe
/// </summary>
public class DataTable
{
    SchemaElement Schema;
    DocumentValue Document;

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
        this.Schema = element;
        this.Document = document;
    }
}