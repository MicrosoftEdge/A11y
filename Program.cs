using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Edge.A11y
{
    class Program
    {
        static void Main(string[] args)
        {
            var testName = args.FirstOrDefault();

            TestStrategy a11yStrategy = new EdgeStrategy();

            var results = TestData.alltests.Value.Where(td =>
                td._ControlType != null && //Control type == null means skip the test
                (testName == null || td._TestName == testName)) //Either no test name was provided or the test names match
                .ToList().ConvertAll(td => a11yStrategy.Execute(td));

            var flatResults = results.Where(r => r.Any()).ToList().ConvertAll(r =>
            {
                var element = r.ElementAt(1);
                element.Result = element.Result == ResultType.Fail && r.ElementAt(0).Result == ResultType.Pass ? ResultType.Half : element.Result;
                element.Name = element.Name.Replace("-2", "");
                return element;
            });

            flatResults.OrderBy(r => r.Result).ToList().ForEach(r => Console.WriteLine(r));

            var scores = flatResults.ConvertAll(r =>
            {
                switch (r.Result)
                {
                    case ResultType.Fail:
                        return 0;
                    case ResultType.Half:
                        return .5;
                    case ResultType.Pass:
                        return 1;
                    default:
                        throw new InvalidDataException();
                }
            });

            if (scores.Any())
            {
                var score = scores.Average();

                Console.WriteLine("Edge Score: " + score * 100);

                ResultsToCSV(flatResults);
            }
            else
            {
                Console.WriteLine("No tests matched the name " + args[0]);
            }

            a11yStrategy.Close();
        }

        public static void ResultsToCSV(List<TestCaseResult> results)
        {
            var filePath = Path.Combine(DriverManager.ProjectRootFolder, "scores.csv");
            var resultline = results.Select(r => r.Result.ToString()).Aggregate((s1, s2) => s1 + "," + s2);
            if (!File.Exists(filePath))
            {
                var headerLine = results.Select(r => r.Name).Aggregate((s1, s2) => s1 + "," + s2) + "\n";
                File.WriteAllText(filePath, headerLine);
            }
            var writer = File.AppendText(filePath);
            writer.WriteLine(resultline);
            writer.Flush();
            writer.Close();
        }
    }
}
