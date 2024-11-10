using Microsoft.VisualBasic;

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
    public MultinomialNaiveBayesModel Fit(DataTable trainingDataset, IEnumerable<string> responses, IEnumerable<string> features = null)
    {
        var parameters = new MultinomialNaiveBayesParameters(trainingDataset, responses);

        // Model for all response columns
        MultinomialNaiveBayesModel model = new MultinomialNaiveBayesModel(parameters);

        // build model for each response variable.
        foreach (var response in parameters.Responses)
        {
            model.CreateModelForResponse(response);

            int instances = 0;  // size of training data

            foreach (var row in trainingDataset.Document)
            {
                instances++;
                string category = row[response];

                // get numbers of each classifier
                if (!model.GetModelForResponse(response).Counts.ContainsKey(category))
                {
                    model.GetModelForResponse(response).Counts[category] = 0;
                    model.GetModelForResponse(response).CountsAfterEvidence[category] = new Dictionary<string, Dictionary<string, int>>();
                    foreach (string key in row.AsDocument.Keys)
                    {
                        model.GetModelForResponse(response).CountsAfterEvidence[category][key] = new Dictionary<string, int>();
                    }
                }

                model.GetModelForResponse(response).Counts[category]++;

                foreach (string key in row.AsDocument.Keys)
                {
                    if (!model.GetModelForResponse(response).CountsAfterEvidence[category][key].ContainsKey(row[key].ToString()))
                    {
                        model.GetModelForResponse(response).CountsAfterEvidence[category][key][row[key]] = 0;
                    }
                    model.GetModelForResponse(response).CountsAfterEvidence[category][key][row[key]]++;
                }
            }
            model.TrainingDataRowCount = instances;
        }
        return model;
    }

    public DataTable Predict(MultinomialNaiveBayesModel model, DataTable testData)
    {
        foreach (var response in model.Parameters.Responses)
        {
            foreach (var row in testData.Document)
            {
                float bestOutcomeScore = 0;
                string bestOutcomeEvent = "";   // the predicted value

                foreach (var key in model.GetModelForResponse(response).Counts.Keys)
                {
                    float currentOutcomeScore = 1;
                    currentOutcomeScore = currentOutcomeScore * ((float)model.GetModelForResponse(response).Counts[key] / model.TrainingDataRowCount);

                    foreach (var item in row.AsDocument.Keys)
                    {
                        float probability = 0;
                        if (model.GetModelForResponse(response).CountsAfterEvidence[key][(string)item].ContainsKey(row[item]))
                        {
                            probability = (float)model.GetModelForResponse(response).CountsAfterEvidence[key][(string)item][row[item]] / (int)model.GetModelForResponse(response).Counts[key];
                        }
                        currentOutcomeScore = currentOutcomeScore * probability;
                    }

                    if (currentOutcomeScore > bestOutcomeScore)
                    {
                        bestOutcomeScore = currentOutcomeScore;
                        bestOutcomeEvent = key;
                    }
                }
                row.AsDocument["predict_" + response] = bestOutcomeEvent; // set predicted value for row
            }
        }
        return testData;
    }
}