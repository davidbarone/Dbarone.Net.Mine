using System.IO;
using System.Linq;
using Dbarone.Net.Mine;
using Xunit;

public class DataTableTests
{

    [Fact]
    public void TestIEnumerable()
    {

    }

    [Fact]
    public void ReadJson()
    {
        var str = Dataset.GetStream(DatasetEnum.foobarbaz);
        var dt = DataTable.ReadJson(str);
        Assert.NotNull(dt);
        Assert.IsType<DataTable>(dt);
        Assert.Equal(2, dt.Columns.Count());
        Assert.Equal(3, dt.Rows.Count());
    }
}