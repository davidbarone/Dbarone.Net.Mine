using Xunit;
using System.IO;
using Dbarone.Net.Csv;
using Dbarone.Net.Document;
using System.Linq;

namespace Dbarone.Net.Mine.Tests;

public class AprioriTests
{
    private DataTable GetData()
    {
        var str = Dataset.GetStream(DatasetEnum.apriori);
        var dt = DataTable.ReadCsv(str);
        return dt;
    }

    [Fact]
    public void AprioriTest()
    {
        var dt = GetData();
        var results = dt.Apriori(0.25, 0.5, "ID", "Item");

        // should be 16 association rules
        Assert.Equal(16, results.Count());

        // 2^3 -> 1 rule:
        // Support for (2,3) = 4/9
        // Support for (1,2,3) = 2/9
        // Confidence = (2/9)/(4/9) = 0.5
        // Lift = Confidence/Support(1,2,3) = 0.5 / (2/9) = 2.25
        var rule = results.FirstOrDefault(r => r.Rule == "I2^I3->I1");
        Assert.NotNull(rule);
        Assert.Equal(0.5, rule.Confidence);
        Assert.Equal(2.25, rule.Lift);
    }
}