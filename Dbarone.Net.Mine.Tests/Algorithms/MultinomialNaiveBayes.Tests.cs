using System.IO;
using Dbarone.Net.Csv;
using Dbarone.Net.Document;
using System.Linq;
using Xunit;

namespace Dbarone.Net.Mine.Tests;

public class MultinomialNaiveBayesTests
{
    public static string Tennis => @"Outlook,Temperature,Humidity,Windy,Play
sunny,hot,high,false,no
sunny,hot,high,true,no
overcast,hot,high,false,yes
rainy,mild,high,false,yes
rainy,cool,normal,false,yes
rainy,cool,normal,true,no
overcast,cool,normal,true,yes
sunny,mild,high,false,no
sunny,cool,normal,false,yes
rainy,mild,normal,false,yes
sunny,mild,normal,true,yes
overcast,mild,high,true,yes
overcast,hot,normal,false,yes
rainy,mild,high,true,no";

    public DataTable GetData()
    {
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(MultinomialNaiveBayesTests.Tennis);
        MemoryStream stream = new MemoryStream(byteArray);

        CsvReader reader = new CsvReader(stream);
        var data = reader.Read();
        var doc = new DocumentArray(data.Select(r => new DocumentValue(r)));
        DataTable dt = new DataTable(doc);
        return dt;
    }

    [Fact]
    public void MultinomialNaiveBayesTest1()
    {
        var data = GetData();
        MultinomialNaiveBayes nb = new MultinomialNaiveBayes();
        var model = nb.Fit(data, ["Play"]);
        var results = nb.Predict(model, data);
    }
}