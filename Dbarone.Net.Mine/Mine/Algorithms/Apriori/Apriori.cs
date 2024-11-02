using System.Collections;

namespace Dbarone.Net.Mine;

/// <summary>
/// The Apriori algorithm is a data mining algorithm used for modelling 'association' (market-basket) problems.
/// 
/// Summary:
/// 1. Algorithm operates on a database that contains a number of transactions
/// 2. Itemset = set of items in a basket (individual transaction)
/// 3. Support = % of transactions for which pattern is true
/// 3. Frequent Itemset = sets of itemsets which has minimum support (denoted 'Li')
/// </summary>
public static class Apriori
{
    public static List<Hashtable> Solve(this DataTable data, double support, double confidence, string transactionColumnName, string itemColumnName)
    {
        int totalTransactions = data.Column(transactionColumnName).Unique().Count();
        int minSupportCnt = (int)((double)totalTransactions * support);

        Dictionary<ItemSet, int> frequentItemsets = new Dictionary<ItemSet, int>();
        Dictionary<ItemSet, List<int>> C0 = new Dictionary<ItemSet, List<int>>();    // C-0 candidate itemsets

        // 1. Get Candidate C0
        foreach (var item in data.Document)
        {
            ItemSet i = new ItemSet(new List<object> { item.AsDocument[itemColumnName] });

            if (!C0.ContainsKey(i))
                C0[i] = new List<int>();

            C0[i].Add(item.AsDocument[transactionColumnName]);
        }

        Dictionary<ItemSet, List<int>> Cn = new Dictionary<ItemSet, List<int>>();    // C-1 candidate itemsets
        foreach (var item in C0.Keys)
        {
            if (C0[item].Count() >= minSupportCnt)
                Cn[item] = C0[item];
        }

        while (Cn.Count() > 0)
        {
            foreach (var item in Cn.Keys)
            {
                frequentItemsets[item] = Cn[item].Count;
            }
            Cn = GenerateCandidates(Cn, minSupportCnt);
        }

        var associationRules = GenerateAssociationRules(frequentItemsets, confidence);

        List<Hashtable> results = new List<Hashtable>();
        foreach (var key in associationRules.Keys)
        {
            Hashtable ht = new Hashtable();
            ht["Itemset"] = key;
            ht["Confidence"] = associationRules[key];
            results.Add(ht);
        }
        return results;
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

    private static Dictionary<ItemSet, List<int>> GenerateCandidates(Dictionary<ItemSet, List<int>> previousCandidates, int minSupportCnt)
    {
        List<ItemSet> newCandidateItemSets = new List<ItemSet>();
        Dictionary<ItemSet, List<int>> newCandidates = new Dictionary<ItemSet, List<int>>();

        foreach (var candidate1 in previousCandidates.Keys)
        {
            foreach (var candidate2 in previousCandidates.Keys)
            {
                var joined = candidate1.Join(candidate2);
                if (joined != null && !newCandidateItemSets.Contains(joined))
                    newCandidateItemSets.Add(joined);
            }
        }

        // Now for each Cn candidate, get the transactions that match
        // 1. Get the two Cn-1 candidates that joined to make the new Cn candidate
        foreach (var newCandidateItemSet in newCandidateItemSets)
        {
            var sourcecandidates = previousCandidates.Keys.Where(a => a.IsSubsetOf(newCandidateItemSet));
            List<int> TID = previousCandidates[sourcecandidates.First()];

            foreach (var sourcecandidate in sourcecandidates)
                TID = TID.Intersect(previousCandidates[sourcecandidate]).ToList();

            newCandidates.Add(newCandidateItemSet, TID);

            // Frequent itemset ?
            if (TID.Count < minSupportCnt)
                newCandidates.Remove(newCandidateItemSet);
        }
        return newCandidates;
    }
}
