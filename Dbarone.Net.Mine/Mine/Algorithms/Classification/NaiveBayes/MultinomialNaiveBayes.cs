using Dbarone.Net.Document;
using Microsoft.VisualBasic;

namespace Dbarone.Net.Mine;

public class MultinomialNaiveBayes
{
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

            foreach (var row in trainingDataset.Rows)
            {
                instances++;
                string category = row[response];

                // get numbers of each classifier
                if (!model.GetModelForResponse(response).Counts.ContainsKey(category))
                {
                    model.GetModelForResponse(response).Counts[category] = 0;
                    model.GetModelForResponse(response).CountsAfterEvidence[category] = new Dictionary<string, Dictionary<string, int>>();
                    foreach (string key in row.Keys)
                    {
                        model.GetModelForResponse(response).CountsAfterEvidence[category][key] = new Dictionary<string, int>();
                    }
                }

                model.GetModelForResponse(response).Counts[category]++;

                foreach (string key in parameters.Features)
                {
                    if (!model.GetModelForResponse(response).CountsAfterEvidence[category][key].ContainsKey(row[key].AsString))
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

    public DataTable Predict(MultinomialNaiveBayesModel model, DataTable testData, string predictColumnPrefix = "predict_")
    {
        List<DictionaryDocument> results = new List<DictionaryDocument>();

        foreach (var row in testData.Rows)
        {
            foreach (var response in model.Parameters.Responses)
            {
                float bestOutcomeScore = 0;
                string bestOutcomeEvent = "";   // the predicted value

                foreach (var key in model.GetModelForResponse(response).Counts.Keys)
                {
                    float currentOutcomeScore = 1;
                    currentOutcomeScore = currentOutcomeScore * ((float)model.GetModelForResponse(response).Counts[key] / model.TrainingDataRowCount);

                    foreach (var item in model.Parameters.Features)
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
                row[$"{predictColumnPrefix}{response}"] = bestOutcomeEvent; // set predicted value for row
            }
            results.Add(row);
        }
        return new DataTable(new DocumentArray(results));
    }
}