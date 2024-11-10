using Dbarone.Net.Mine;

public class MultinomialNaiveBayesModel
{
    Dictionary<string, MultinomialNaiveBayesResponseModel> _model = new Dictionary<string, MultinomialNaiveBayesResponseModel>();
    public MultinomialNaiveBayesParameters Parameters { get; set; }

    public MultinomialNaiveBayesModel(MultinomialNaiveBayesParameters parameters)
    {
        this.Parameters = parameters;
    }

    public MultinomialNaiveBayesResponseModel GetModelForResponse(string response)
    {
        return _model[response];
    }

    public bool ModelExistsForResponse(string response)
    {
        return _model.ContainsKey(response);
    }

    public void CreateModelForResponse(string response)
    {
        _model[response] = new MultinomialNaiveBayesResponseModel();
    }

    public int TrainingDataRowCount { get; set; }
}