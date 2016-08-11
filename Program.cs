﻿namespace Microsoft.Edge.A11y
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Win32;

    /// <summary>
    /// The main entry point for A11y
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">The command line arguments to the program</param>
        public static void Main(string[] args)
        {
            var testName = args.FirstOrDefault();

            TestStrategy a11yStrategy = new EdgeStrategy(fileSuffix: ".html");

            var results = TestData.AllTests()
                .Where(td => (testName == null || td.TestName == testName)) // Either no test name was provided or the test names match
                .Select(td => a11yStrategy.Execute(td).ToList()) // Execute each of the tests
                .Where(r => r.Any()) // Only keep the ones that were executed
                .Select(r => // Convert results from internal form (Pass/Pass, Pass/Fail, Fail/Fail) to external (Pass, Half, Fail)
                {
                    var first = r.ElementAt(0);
                    var second = r.ElementAt(1);
                    second.Result = second.Result == ResultType.Fail && first.Result == ResultType.Pass ? ResultType.Half : second.Result;
                    second.Name = second.Name.Replace("-2", string.Empty);
                    return second;
                })
                .ToList();

            // output results to the console: failures, then halves, then passes
            results.OrderBy(r => r.Result == ResultType.Pass)
                .ThenBy(r => r.Result == ResultType.Half)
                .ToList()
                .ForEach(r => Console.WriteLine(r));

            if (results.Any())
            {
                var score = results.Select(r =>
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
                }).Average() * 100;

                Console.WriteLine("Edge Score: " + score);

                ResultsToCSV(results, score);
            }
            else
            {
                Console.WriteLine("No tests matched the name " + args[0]);
            }

            a11yStrategy.Close();
        }

        /// <summary>
        /// Prints all results to a csv file
        /// </summary>
        /// <param name="results">The results to save</param>
        /// <param name="score">The overall score</param>
        public static void ResultsToCSV(List<TestCaseResult> results, double score)
        {
            // Get the file
            var filePath = Path.Combine(DriverManager.ProjectRootFolder, "scores.csv");

            // If this is the first time, write the header line with the test names
            if (!File.Exists(filePath))
            {
                var headerLine = "buildNumber,buildIteration,buildArchitecture,buildBranch,buildDate,score,time," +
                    results.Select(r => r.Name + "," + r.Name + "-details").Aggregate((s1, s2) => s1 + "," + s2) + "\n";
                File.WriteAllText(filePath, headerLine);
            }

            var build = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "BuildLabEx", null) as string;
            if (build == null)
            {
                throw new Exception("Unable to get build string");
            }

            var time = DateTime.Now.ToString("yyyyMMdd-HHmm");

            // Write the results
            var writer = File.AppendText(filePath);
            var resultline = build.Replace('.', ',') + "," + score + "," + time + "," +
                results.Select(r => r.Result.ToString() + "," + (r.MoreInfo != null ? r.MoreInfo.Replace('\n', '\t') : string.Empty))
                .Aggregate((s1, s2) => s1 + "," + s2);
            writer.WriteLine(resultline);

            writer.Flush();
            writer.Close();
        }
    }
}
