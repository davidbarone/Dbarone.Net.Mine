using Dbarone.Net.Mine;
using Xunit;

public class DatasetTests
{

    [Fact]
    public void GetDatasetString()
    {
        var actual = Dataset.GetString(DatasetEnum.foobarbaz);
        Assert.NotNull(actual);
        Assert.Equal(208, actual.Length);
    }
}