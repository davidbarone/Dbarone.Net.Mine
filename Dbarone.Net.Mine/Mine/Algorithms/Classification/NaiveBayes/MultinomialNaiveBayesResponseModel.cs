/// <summary>
/// Multinomial Naive Bayes model for single response vector / column.
/// </summary>
public class MultinomialNaiveBayesResponseModel
{
    /// <summary>
    /// Stores counts of the priori events.
    /// </summary>
    public Dictionary<string, int> Counts = new Dictionary<string, int>();

    /// <summary>
    /// Stores counts of the posterior events.
    /// </summary>
    public Dictionary<string, Dictionary<string, Dictionary<string, int>>> CountsAfterEvidence = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
}