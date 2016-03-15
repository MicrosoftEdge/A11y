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
            var driver = new DriverManager(TimeSpan.Zero);
            var a11yStrategy = new EdgeStrategy(driver);

            var results = TestData.alltests.Value.Where(td => td._ControlType != null).ToList().ConvertAll(td => a11yStrategy.Execute(driver, td));

            var flatResults = results.Where(r => r.Any()).ToList().ConvertAll(r =>
            {
                var element = r.ElementAt(1);
                element.Result = element.Result == ResultType.Fail && r.ElementAt(0).Result == ResultType.Pass ? ResultType.Half : element.Result;
                element.Name = element.Name.Replace("-2", "");
                return element;
            });

            flatResults.OrderBy(r => r.Result).ToList().ForEach(r => Console.WriteLine(r.ToString()));

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

            var score = scores.Average() * (scores.Count / 39.0);

            Console.WriteLine("Edge Score: " + score * 100);

            ResultsToCSV(flatResults);

            driver.Close();
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
