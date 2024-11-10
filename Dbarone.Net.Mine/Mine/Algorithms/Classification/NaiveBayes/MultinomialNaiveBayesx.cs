using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Dbarone.Net.Mine
{
    public class NaiveBayes
    {

        public IEnumerable<Hashtable> Execute(IEnumerable<Hashtable> trainingData, IEnumerable<Hashtable> testData)
        {
            Data data = new Data(trainingData.ToList(), testData.ToList());

        }
            foreach (var row in data.TestData)
                yield return row;
        }
}


    /*
        [Serializable]
        [Documentation("Applies the Naive Bayes classifier to an in-memory results. Classifies data according to the statistics based on a training data set.")]
        public class DmNaiveBayesTask : Task, IMemoryReaderTask, IMemoryWriterTask
        {
            public string ReturnResultset { get; set; }

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
    */

}
