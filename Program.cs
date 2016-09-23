using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Win32;

namespace Microsoft.Edge.A11y
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                HttpListener httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://127.0.0.1:4119/");
                httpListener.Start();
                HttpListenerContext context = httpListener.GetContext();
                HttpListenerRequest request = context.Request;
                StreamReader stream = new StreamReader(request.InputStream);
                string input = stream.ReadToEnd();
                HttpListenerResponse response = context.Response;

                Console.WriteLine("RECEIVED: " + input);

                var alltests = JSONParser.SampleJsonTest(input);

                TestStrategy a11yStrategy = new EdgeStrategy(repositoryPath: @"D:\devel\A11y\", fileSuffix: ".html");

                var results = alltests
                    .ToList().ConvertAll(td => a11yStrategy.Execute(td)) //Execute each of the tests
                    .Where(r => r.Any()) //Only keep the ones that were executed
                    .ToList().ConvertAll(r => //Convert results from internal form (Pass/Pass, Pass/Fail, Fail/Fail) to external (Pass, Half, Fail)
                    {
                        var first = r.ElementAt(0);
                        var second = r.ElementAt(1);
                        second.Result = second.Result == ResultType.Fail && first.Result == ResultType.Pass ? ResultType.Half : second.Result;
                        second.Name = second.Name.Replace("-2", "");
                        return second;
                    });

                //output results to the console: failures, then halves, then passes
                results.OrderBy(r => r.Result == ResultType.Pass).ThenBy(r => r.Result == ResultType.Half).ToList().ForEach(r => Console.WriteLine(r));

                string responseString = "";
                byte[] buffer;
                double score = 0;
                if (results.Any())
                {
                    score = results.ConvertAll(r =>
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

                responseString = JSONParser.ResultsToJSON(results, score);

                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                httpListener.Close();
                a11yStrategy.Close();
            }
        }

        public static void ResultsToCSV(List<TestCaseResult> results, double score)
        {
            //Get the file
            var filePath = Path.Combine(DriverManager.ProjectRootFolder, "scores.csv");

            //If this is the first time, write the header line with the test names
            if (!File.Exists(filePath))
            {
                var headerLine = "buildNumber,buildIteration,buildArchitecture,buildBranch,buildDate,score,time," +
                    results.Select(r => r.Name + "," + r.Name + "-details").Aggregate((s1, s2) => s1 + "," + s2) + "\n";
                File.WriteAllText(filePath, headerLine);
            }

            var build = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "BuildLabEx", null);
            if (build == null || build as string == null)
            {
                throw new Exception("Unable to get build string");
            }

            var time = DateTime.Now.ToString("yyyyMMdd-HHmm");

            //Write the results
            var writer = File.AppendText(filePath);
            var resultline = (build as string).Replace('.', ',') + "," + score + "," + time + "," +
                results.Select(r => r.Result.ToString() + "," + (r.MoreInfo != null ? r.MoreInfo.Replace('\n', '\t') : ""))
                .Aggregate((s1, s2) => s1 + "," + s2);
            writer.WriteLine(resultline);

            writer.Flush();
            writer.Close();
        }
    }
}
