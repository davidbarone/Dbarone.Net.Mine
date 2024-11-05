using System.Collections;
using Dbarone.Net.Document;

namespace Dbarone.Net.Mine;

/// <summary>
/// The Apriori algorithm is a data mining algorithm used for modelling 'association' (market-basket) problems.
/// 
/// Summary:
/// 1. Algorithm operates on a database that contains a number of transactions
/// 2. Itemset = set of items in a basket (individual transaction)
/// 3. Support = % of transactions for which pattern is true
/// 3. Frequent Itemset = sets of itemsets which has minimum support (denoted 'Li')
///
/// Sources:
/// - https://en.wikipedia.org/wiki/Apriori_algorithm
/// - https://towardsdatascience.com/apriori-association-rule-mining-explanation-and-python-implementation-290b42afdfc6
/// - https://kaumadiechamalka100.medium.com/apriori-algorithm-examples-be8915b01cf2
/// - https://www.geeksforgeeks.org/apriori-algorithm/ (This one used for test data case)
/// </summary>
public static class AprioriExtension
{
    /// <summary>
    /// Calculates association rules on a data table.
    /// </summary>
    /// <param name="table">The data table to calculation association rules for.</param>
    /// <param name="support">The minimum support level (0..1).</param>
    /// <param name="confidence">The minimum confidence level (0..1).</param>
    /// <param name="transactionColumnName">The column name in the table storing the transaction identifier.</param>
    /// <param name="itemColumnName">The column name in the table storing the item identifier.</param>
    /// <returns></returns>
    public static List<Hashtable> Apriori(this DataTable table, double support, double confidence, string transactionColumnName, string itemColumnName)
    {
        // format data into array of BasketItem objects for easier processing.
        List<BasketItem> baskets = new List<BasketItem>();
        foreach (DocumentValue row in table.Document)
        {
            BasketItem bi = new BasketItem
            {
                TID = row.AsDocument[transactionColumnName],
                Item = row.AsDocument[itemColumnName]
            };
            baskets.Add(bi);
        }

        // solve
        return AprioriExtension.Solve(baskets, support, confidence, transactionColumnName, itemColumnName);
    }

    private static List<ItemSet> GetCandidateItemSet(int k, IEnumerable<BasketItem> baskets)
    {
        List<ItemSet> candidateItemset = new List<ItemSet>();

        if (k == 1)
        {
            // base / trivial case
            foreach (var item in baskets)
            {
                ItemSet i = new ItemSet(new List<object> { item.Item });

                if (!candidateItemset.Contains(i))
                {
                    candidateItemset.Add(i);
                }
            }
            return candidateItemset;
        }
        else
        {
            // recursive case
            List<ItemSet> newCandidateItemSets = new List<ItemSet>();

            foreach (var candidate1 in GetCandidateItemSet(k - 1, baskets))
            {
                foreach (var candidate2 in GetCandidateItemSet(k - 1, baskets))
                {
                    var joined = candidate1.Join(candidate2);
                    if (joined != null && !newCandidateItemSets.Contains(joined))
                        newCandidateItemSets.Add(joined);
                }
            }
            return newCandidateItemSets;
        }
    }

    private static Dictionary<ItemSet, List<string>> GetFrequentItemSets(int k, IEnumerable<BasketItem> baskets, int minSupportCount)
    {
        Dictionary<ItemSet, List<string>> frequentItemSets = new Dictionary<ItemSet, List<string>>();
        var candidates = GetCandidateItemSet(k, baskets);

        if (k == 1)
        {
            Dictionary<ItemSet, List<string>> temp = new Dictionary<ItemSet, List<string>>();
            foreach (var item in candidates)
            {
                temp[item] = new List<string>();
            }

            // base / trivial case
            foreach (var item in baskets)
            {
                ItemSet i = new ItemSet(new List<object> { item.Item });
                // Add the transaction id occurrence to the candidate itemset for k=1.
                temp[i].Add(item.TID);
            }

            // base / trivial case
            foreach (var key in temp.Keys)
            {
                if (temp[key].Count() >= minSupportCount)
                {
                    frequentItemSets[key] = temp[key];
                }
            }
            return frequentItemSets;
        }
        else
        {
            var previousCandidates = GetFrequentItemSets(k - 1, baskets, minSupportCount);

            // Now for each Cn candidate, get the transactions that match
            // 1. Get the two Cn-1 candidates that joined to make the new Cn candidate
            foreach (var newCandidateItemSet in candidates)
            {
                var sourcecandidates = previousCandidates.Keys.Where(a => a.IsSubsetOf(newCandidateItemSet));
                if (sourcecandidates.Any())
                {
                    List<string> TID = previousCandidates[sourcecandidates.First()];

                    foreach (var sourcecandidate in sourcecandidates)
                        TID = TID.Intersect(previousCandidates[sourcecandidate]).ToList();

                    if (TID.Count >= minSupportCount)
                    {
                        frequentItemSets.Add(newCandidateItemSet, TID);
                    }
                    else
                    {
                        // not frequent
                    }
                }
            }
            return frequentItemSets;
        }
    }

    private static List<Hashtable> Solve(IEnumerable<BasketItem> baskets, double support, double confidence, string transactionColumnName, string itemColumnName)
    {
        Dictionary<ItemSet, int> results = new Dictionary<ItemSet, int>();

        // Total transactions
        int totalTransactions = baskets.Select(d => d.TID).Distinct().Count();

        // minimum support count = support % * totalTransactions
        // support of item I is defined as the number of transactions containing I divided by the total number of transactions.
        int minSupportCnt = (int)((double)totalTransactions * support);

        var frequentItemSets = GetFrequentItemSets(1, baskets, minSupportCnt);
        foreach (var key in frequentItemSets.Keys)
        {
            results[key] = frequentItemSets[key].Count();
        }

        for (int k = 2; frequentItemSets.Keys.Count() > 0; k++)
        {
            // frequentItemSets 
            frequentItemSets = GetFrequentItemSets(k, baskets, minSupportCnt);
            foreach (var key in frequentItemSets.Keys)
            {
                results[key] = frequentItemSets[key].Count();
            }
        }

        var associationRules = GenerateAssociationRules(results, confidence);

        List<Hashtable> lht = new List<Hashtable>();
        foreach (var key in associationRules.Keys)
        {
            Hashtable ht = new Hashtable();
            ht["Itemset"] = key;
            ht["Confidence"] = associationRules[key];
            lht.Add(ht);
        }
        return lht;
    }

    private static Dictionary<string, double> GenerateAssociationRules(Dictionary<ItemSet, int> frequentItemsets, double confidenceThreshold)
    {
        Dictionary<string, double> associationRules = new Dictionary<string, double>();

        foreach (var frequentItemSeta in frequentItemsets.Keys)
        {
            foreach (var frequentItemSetb in frequentItemsets.Keys)
            {
                if (frequentItemSeta.IsSubsetOf(frequentItemSetb))
                {
                    double a = frequentItemsets[frequentItemSeta];
                    double b = frequentItemsets[frequentItemSetb];
                    var confidence = b / a;
                    if (confidence >= confidenceThreshold)
                    {
                        // add rule
                        // format = substring (I1^I2^I3...) -> superstring
                        // ie association from 'I1,I2' to 'I1,I2,I5' is written as: 'I1^I2->I5'

                        string rule = string.Format(
                            "{0}->{1}",
                            string.Join("^", frequentItemSeta.Values.ToList()),
                            string.Join("^", frequentItemSetb.Values.Except(frequentItemSeta.Values).ToList()));
                        associationRules[rule] = confidence;
                    }
                }
            }
        }
        return associationRules;
    }
}
