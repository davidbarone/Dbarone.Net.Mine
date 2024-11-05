using Xunit;
using System.IO;
using Dbarone.Net.Csv;
using Dbarone.Net.Document;
using System.Linq;

namespace Dbarone.Net.Mine.Tests;

public class AprioriTests
{
    public static string Data => @"ID,Item
T1,I1
T1,I2
T1,I5
T2,I2
T2,I4
T3,I2
T3,I3
T4,I1
T4,I2
T4,I4
T5,I1
T5,I3
T6,I2
T6,I3
T7,I1
T7,I3
T8,I1
T8,I2
T8,I3
T8,I5
T9,I1
T9,I2
T9,I3";

    [Fact]
    public void AprioriTest()
    {
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(AprioriTests.Data);
        MemoryStream stream = new MemoryStream(byteArray);

        CsvReader reader = new CsvReader(stream);
        var data = reader.Read();
        var doc = new DocumentArray(data.Select(r => new DocumentValue(r)));
        DataTable dt = new DataTable(doc);
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