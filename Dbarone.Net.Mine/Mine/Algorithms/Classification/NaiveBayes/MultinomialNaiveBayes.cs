namespace Dbarone.Net.Mine;

public class MultinomialNaiveBayes
{
    private IEnumerable<string> _features { get; set; }
    private IEnumerable<string> _responses { get; set; }

    public MultinomialNaiveBayes()
    {

    }

    /// <summary>
    /// Fits a multinomial naive bayes model to a training dataset.
    /// </summary>
    /// <param name="trainingDataset">The training dataset</param>
    /// <param name="responses">The predictor / y variable names. Required.</param>
    /// <param name="features">The feature / x variable names in the dataset.</param>
    public Dictionary<string, MultinomialNaiveBayesModel> Fit(DataTable trainingDataset, IEnumerable<string> responses, IEnumerable<string> features = null)
    {
        var parameters = new MultinomialNaiveBayesParameters(trainingDataset, responses);

        // Model for all response columns
        Dictionary<string, MultinomialNaiveBayesModel> model = new Dictionary<string, MultinomialNaiveBayesModel>();

        // build model for each response variable.
        foreach (var response in parameters.Responses)
        {
            model[response] = new MultinomialNaiveBayesModel();

            int instances = 0;  // size of training data

            foreach (var row in trainingDataset.Document)
            {
                instances++;
                string category = row[response];

                // get numbers of each classifier
                if (!model[response].Counts.ContainsKey(category))
                {
                    model[response].Counts[category] = 0;
                    model[response].CountsAfterEvidence[category] = new Dictionary<string, Dictionary<string, int>>();
                    foreach (string key in row.AsDocument.Keys)
                    {
                        model[response].CountsAfterEvidence[category][key] = new Dictionary<string, int>();
                    }
                }

                model[response].Counts[category]++;

                foreach (string key in row.AsDocument.Keys)
                {
                    if (!model[response].CountsAfterEvidence[category][key].ContainsKey(row[key].ToString()))
                    {
                        model[response].CountsAfterEvidence[category][key][row[key]] = 0;
                    }
                    model[response].CountsAfterEvidence[category][key][row[key]]++;
                }
            }
        }
        return model;
    }

    public DataTable Predict(Dictionary<string, MultinomialNaiveBayesModel> model, DataTable testData)
    {
        // training set now completed - do test
        foreach (var row in data.TestData)
        {
            float bestOutcomeScore = 0;
            object bestOutcomeEvent = "";

            foreach (var key in count_events.Keys)
            {
                float currentOutcomeScore = 1;
                currentOutcomeScore = currentOutcomeScore * ((float)count_events[key] / instances);

                foreach (var item in row.Keys)
                {
                    float probability = 0;
                    if (count_event_after_evidence[key][(string)item].ContainsKey(row[item]))
                    {
                        probability = (float)count_event_after_evidence[key][(string)item][row[item]] / (int)count_events[key];
                    }
                    currentOutcomeScore = currentOutcomeScore * probability;
                }

                // Console.WriteLine(string.Format("{0} = {1}", currentOutcomeScore, key));

                if (currentOutcomeScore > bestOutcomeScore)
                {
                    bestOutcomeScore = currentOutcomeScore;
                    bestOutcomeEvent = key;
                }
            }
            row[eventField] = bestOutcomeEvent; // set predicted value for row
                                                //Console.WriteLine(string.Format("outcome is: {0} with an outcome of: {1}", bestOutcomeEvent, bestOutcomeScore));
        }

    }
}