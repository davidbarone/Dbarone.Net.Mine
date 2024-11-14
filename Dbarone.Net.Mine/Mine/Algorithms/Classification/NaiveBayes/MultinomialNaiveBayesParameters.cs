using System.Data;

namespace Dbarone.Net.Mine;

/// <summary>
/// Data for the NaiveBayes algorithm
/// </summary>
public class MultinomialNaiveBayesParameters
{
    IEnumerable<string> _responses { get; set; }
    DataTable _trainingData { get; set; }

    /// <summary>
    /// Parameters used for Naive Bayes
    /// </summary>
    /// <param name="trainingData">The training data to use.</param>
    /// <param name="responses">The predictor / y / responses variable names.</param>
    public MultinomialNaiveBayesParameters(DataTable trainingData, IEnumerable<string> responses)
    {
        _trainingData = trainingData;
        _responses = responses;
    }

    /// <summary>
    /// The field storing the event to be predicted
    /// </summary>
    public IEnumerable<string> Responses
    {
        get
        {
            return _responses;
        }
    }

    /// <summary>
    /// The fields storing the evidences for each instance
    /// </summary>
    public IEnumerable<string> Features
    {
        get
        {
            foreach (var column in _trainingData.Columns)
            {
                if (!Responses.Contains(column))
                {
                    yield return column;
                }
            }
        }
    }
}
