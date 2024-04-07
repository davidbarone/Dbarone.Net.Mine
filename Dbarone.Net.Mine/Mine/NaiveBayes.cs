using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dbarone.Etl.Configuration.Attributes;
using System.Collections;
using Dbarone.Etl.Tasks;
using Dbarone.Etl.Tasks.Interfaces;

namespace Dbarone.Etl.DataMining.Tasks
{
    public class NaiveBayes
    {
        /// <summary>
        /// Data for the NaiveBayes algorithm
        /// </summary>
        private class Data
        {
            public Data(IEnumerable<Hashtable> trainingData, IEnumerable<Hashtable> testData)
            {
                TrainingData = trainingData;
                TestData = testData;
            }

            /// <summary>
            /// The training data
            /// </summary>
            public IEnumerable<Hashtable> TrainingData { get; private set; }

            /// <summary>
            /// The test data
            /// </summary>
            public IEnumerable<Hashtable> TestData { get; private set; }

            /// <summary>
            /// The field storing the event to be predicted
            /// </summary>
            public IEnumerable<string> EventFields
            {
                get
                {
                    var keys = TrainingData.First().Keys;
                    foreach (var key in keys)
                        if (!TestData.First().ContainsKey(key))
                            yield return key.ToString();
                }
            }

            /// <summary>
            /// The fields storing the evidences for each instance
            /// </summary>
            public IEnumerable<string> AttributeFields
            {
                get
                {
                    var keys = TrainingData.First().Keys;
                    foreach (var key in keys)
                        if (TestData.First().ContainsKey(key))
                            yield return key.ToString();
                }
            }
        }

        public IEnumerable<Hashtable> Execute(IEnumerable<Hashtable> trainingData, IEnumerable<Hashtable> testData)
        {
            Data data = new Data(trainingData.ToList(), testData.ToList());

            foreach (var eventField in data.EventFields)
            {
                // PRIORI probabilities
                Dictionary<object, int> count_events = new Dictionary<object, int>();

                // POSTERIOR probabilities
                Dictionary<object, Dictionary<string, Dictionary<object, int>>> count_event_after_evidence = new Dictionary<object, Dictionary<string, Dictionary<object, int>>>();

                int instances = 0;  // size of training data

                foreach (var instance in data.TrainingData)
                {
                    instances++;
                    object eventValue = instance[eventField];

                    // get numbers of each classifier
                    if (!count_events.ContainsKey(eventValue))
                    {
                        count_events[eventValue] = 0;
                        count_event_after_evidence[eventValue] = new Dictionary<string, Dictionary<object, int>>();
                        foreach (string key in instance.Keys)
                        {
                            count_event_after_evidence[eventValue][key] = new Dictionary<object, int>();
                        }
                    }

                    count_events[eventValue] += 1;

                    foreach (string key in instance.Keys)
                    {
                        if (!count_event_after_evidence[eventValue][key].ContainsKey(instance[key].ToString()))
                        {
                            count_event_after_evidence[eventValue][key][instance[key]] = 0;
                        }
                        count_event_after_evidence[eventValue][key][instance[key]]++;
                    }
                }

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
            foreach (var row in data.TestData)
                yield return row;
        }
    }

    [Serializable]
    [Documentation("Applies the Naive Bayes classifier to an in-memory results. Classifies data according to the statistics based on a training data set.")]
    public class DmNaiveBayesTask : Task, IMemoryReaderTask, IMemoryWriterTask
    {
        public string ReturnResultset {get; set;}

        public string InResultset { get; set; }

        [PackageConfiguration]
        public string TrainingResultset { get; set; }

        protected override void ExecuteHandler(ProcessEnvironment environment)
        {
            environment.Resultsets[ReturnResultset] = GetData(environment);
        }

        private IEnumerable<Hashtable> GetData(ProcessEnvironment environment)
        {
            NaiveBayes naiveBayes = new NaiveBayes();

            var trainingData = environment.GetResultset(TrainingResultset);
            var testData = environment.GetResultset(InResultset);

            foreach (var result in naiveBayes.Execute(trainingData, testData))
                yield return result;
        }
    }
}
