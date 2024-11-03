using Xunit;

namespace Dbarone.Net.Mine.Tests;

public class DataColumnTests
{
    [Fact]
    public void CreateDataColumn()
    {
        var dc = new DataColumn([1, 2, 3, 4, 5]);
        Assert.Equal(dc.Count(), 5);
    }

    [Fact]
    public void DataColumnUnique()
    {
        var dc = new DataColumn([1, 2, 2, 3, 4, 5]);
        Assert.Equal(dc.Count(), 6);
        Assert.Equal(dc.Unique().Count(), 5);
    }

}