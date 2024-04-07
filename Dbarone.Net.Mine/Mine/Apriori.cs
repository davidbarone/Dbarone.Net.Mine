using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dbarone.Etl.Configuration;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using Dbarone.Etl.Tasks;
using Dbarone.Etl.Configuration.Attributes;
using Dbarone.Etl.Tasks.Interfaces;

namespace Dbarone.Etl.DataMining.Tasks
{
    public class RawData
    {
        public int TID;
        public string Item;
    }

    public class ItemSet
    {
        public ItemSet(IEnumerable<object> items)
        {
            Values = items;
        }

        public IEnumerable<object> Values { get; set; }

        public int Size
        {
            get { return Values.Count(); }
        }

        public ItemSet Join(ItemSet b)
        {
            if (this.Size != b.Size)
                throw new ApplicationException("Itemsets being joined must be of same size");

            // if (size-1) values are same, then can join. Otherwise cannot
            if (this.Values.Intersect(b.Values).Count() != this.Size - 1)
                return null;
            else
                return new ItemSet(this.Values.Union(b.Values));
        }

        public override string ToString()
        {
            return string.Join("->", Values.OrderBy(s => s).ToArray());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode().Equals(obj.GetHashCode());
        }

        /// <summary>
        /// Returns true if the superset specified contains all elements in the current ItemSet.
        /// </summary>
        /// <param name="superSet"></param>
        /// <returns></returns>
        public bool IsSubsetOf(ItemSet superSet)
        {
            return (superSet.Values.Intersect(this.Values).Count() == this.Values.Count() && this.Size < superSet.Size);
        }
    }

    /// <summary>
    /// The Apriori algorithm is a data mining algorithm used for modelling 'association' (market-basket) problems.
    /// 
    /// Summary:
    /// 1. Algorithm operates on a database that contains a number of transactions
    /// 2. Itemset = set of items in a basket (individual transaction)
    /// 3. Support = % of transactions for which pattern is true
    /// 3. Frequent Itemset = sets of itemsets which has minimum support (denoted 'Li')
    /// </summary>
    public class Apriori
    {
        public Dictionary<string, double> Solve(IEnumerable<RawData> data, double supportPct, double confidenceThreshold)
        {
            int totalTransactions = data.Select(d => d.TID).Distinct().Count();
            int minSupportCnt = (int)((double)totalTransactions * supportPct);

            Dictionary<ItemSet, int> frequentItemsets = new Dictionary<ItemSet, int>();

            Dictionary<ItemSet, List<int>> C0 = new Dictionary<ItemSet, List<int>>();    // C-0 candidate itemsets

            // 1. Get Candidate C0
            foreach (var item in data)
            {
                ItemSet i = new ItemSet(new List<object> { item.Item });

                if (!C0.ContainsKey(i))
                    C0[i] = new List<int>();

                C0[i].Add(item.TID);
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

            return GenerateAssociationRules(frequentItemsets, confidenceThreshold);
        }

        private Dictionary<string, double> GenerateAssociationRules(Dictionary<ItemSet, int> frequentItemsets, double confidenceThreshold)
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

        private Dictionary<ItemSet, List<int>> GenerateCandidates(Dictionary<ItemSet, List<int>> previousCandidates, int minSupportCnt)
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

    /// <summary>
    /// Data Mining Task using the Apriori algorithm. This is useful for Associative / Market Basket analysis.
    /// </summary>
    /// <remarks>
    /// Analyses items in a basket (single transaction) for relationships between items
    /// </remarks>
    [Serializable]
    [Documentation("Applies the Apriori data mining algoritm to an in-memory resultset. Useful for market basket analysis.")]
    public class DmAprioriTask : Task, IMemoryReaderTask, IMemoryWriterTask 
    {
        public string InResultset { get; set; }
        public string ReturnResultset { get; set; }

        [PackageConfiguration]
        public string TransactionColumnName { get; set; }   // field storing the transaction identifier

        [PackageConfiguration]
        public string ItemColumnName { get; set; }  // field storing the item name

        [PackageConfiguration]
        public double Support { get; set; }         // the support threshold

        [PackageConfiguration]
        public double Confidence { get; set; }      // the confidence threshold

        private IEnumerable<RawData> GetTrainingData(IEnumerable<Hashtable> inData)
        {
            foreach (var row in inData)
            {
                yield return new RawData { TID = (int)row[TransactionColumnName], Item = (string)row[ItemColumnName] };
            }
        }

        protected override void ExecuteHandler(ProcessEnvironment environment)
        {
            environment.Resultsets[ReturnResultset] = GetData(environment);
        }

        private IEnumerable<Hashtable> GetData(ProcessEnvironment environment)
        {
            List<Hashtable> ret = new List<Hashtable>();

            Apriori apriori = new Apriori();
            var trainingData = GetTrainingData(environment.GetResultset(InResultset));
            var results = apriori.Solve(trainingData, Support, Confidence);
            foreach (var key in results.Keys)
            {
                Hashtable ht = new Hashtable();
                ht["Itemset"] = key;
                ht["Confidence"] = results[key];
                ret.Add(ht);
            }
            return ret;
        }
    }
}
