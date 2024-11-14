using System.IO;
using Dbarone.Net.Mine;
using Xunit;

public class DatasetTests
{
    [Fact]
    public void GetString()
    {
        var actual = Dataset.GetString(DatasetEnum.foobarbaz);
        Assert.NotNull(actual);
        Assert.Equal(208, actual.Length);
    }

    [Fact]
    public void GetStream()
    {
        var actual = Dataset.GetStream(DatasetEnum.foobarbaz);
        Assert.NotNull(actual);
        Assert.IsAssignableFrom<Stream>(actual);
    }
}