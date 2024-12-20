using System.IO;
using Dbarone.Net.Csv;
using Dbarone.Net.Document;
using System.Linq;
using Xunit;

namespace Dbarone.Net.Mine.Tests;

public class MultinomialNaiveBayesTests
{
    public DataTable GetData()
    {
        var str = Dataset.GetStream(DatasetEnum.tennis);
        DataTable dt = DataTable.ReadCsv(str);
        return dt;
    }

    [Fact]
    public void MultinomialNaiveBayesTest()
    {
        var data = GetData();
        MultinomialNaiveBayes nb = new MultinomialNaiveBayes();
        var model = nb.Fit(data, ["Play"]);
        var results = nb.Predict(model, data, "predict_");
        var total = 0.0;
        var correct = 0.0;
        foreach (var row in results.Rows)
        {
            total++;
            var actual = row["Play"].AsString;
            var expected = row["predict_Play"].AsString;
            if (actual == expected)
            {
                correct++;
            }
        }
        // Should predict 13/14 correctly.
        Assert.Equal(13.0 / 14.0, correct / total);
    }
}