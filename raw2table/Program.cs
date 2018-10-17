using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace raw2table
{
    class Program
    {
        static void WriteProcessedMatrix(List<double>[,] arr, string selfTestOptions, string valueExp, string file)
        {
            var sw = new StreamWriter(file + ".proced.csv");
            var dim = arr.GetLength(0);
            List<double>[] singleNodeCosts = new List<double>[dim];
            var matrix = new double[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                singleNodeCosts[i] = arr[i, i];
                matrix[i, i] = GetValue(singleNodeCosts[i], "avg");
            }
            for (int lineIdx = 0; lineIdx < dim; lineIdx++)
            {
                for (int colIdx = 0; colIdx < dim; colIdx++)
                {
                    matrix[lineIdx, colIdx] = GetValue(arr[lineIdx, colIdx], valueExp);
                    if (arr[lineIdx, colIdx].Count == 0 && lineIdx < colIdx)
                    {
                        Console.WriteLine("[Warning] no data from {0} to {1} can be found.", hosts[lineIdx], hosts[colIdx]);
                    }
                    if (selfTestOptions == "true")
                    {
                        matrix[lineIdx, colIdx] -= (matrix[lineIdx, lineIdx] + matrix[colIdx, colIdx]) / 2;
                    }
                    else if (selfTestOptions == "false")
                    {

                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    if (matrix[lineIdx, colIdx] == 0 && arr[lineIdx, colIdx].Count != 0)
                    {
                        Console.WriteLine("[Warning] matrix[{0}, {1}] == 0 observed", hosts[lineIdx], hosts[colIdx]);

                    }
                    sw.Write(matrix[lineIdx, colIdx]);
                    if (colIdx != dim - 1)
                    {
                        sw.Write(',');
                    }
                }
                if (lineIdx != dim - 1)
                {
                    sw.WriteLine();
                }
            }
            //write back
            sw.Dispose();
        }
        static double GetValue(List<double> history, string valExtractExp)
        {
            if (history.Count == 0) return 0;
            if (valExtractExp == "max")
            {
                return history.Max();
            }
            else if (valExtractExp == "min")
            {
                return history.Min();
            }
            else if (valExtractExp == "avg")
            {
                return history.Average();
            }
            else
            {
                Debug.Assert(false);
            }
            return 0;
        }
        static string GetControlOption(string content, int idx, string trim)
        {
            var controlLine = content;
            var controls = controlLine.Split(';');
            return controls[idx].Replace(trim + ":", "").ToLowerInvariant();
        }
        static string InferBenchmark(string content)
        {
            return GetControlOption(content, 0, "BENCHMARK");
        }
        static string SelfTestOption(string content)
        {
            return GetControlOption(content, 1, "SELF_TEST_OPTION");
        }
        static int GetDimension(string content)
        {
            return int.Parse(GetControlOption(content, 2, "DIMENSION"));
        }
        static string ValueExtractionExp(string content)
        {
            return GetControlOption(content, 3, "VALUE");
        }

        static int PreprocessOptions(string content)
        {
            return int.Parse(GetControlOption(content, 4, "PREPROCESS"));
        }
        static Regex IdentityDiscoveryRegex(string benchmark)
        {
            string regexStr = null;
            if (benchmark == "tcp-all-all")
            {
                regexStr = @"\d+,(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b),\d+,(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b),\d+,\d+,";
            }
            else if ( benchmark == "igi")
            {
                regexStr = @"STARTING (\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b)-(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b)";
            }
            else if (benchmark == "ping" || benchmark == "dpdk-echo")
            {
                regexStr = @"(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b),(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b)";
            }
            return new Regex(regexStr);
        }

        static Regex ValueExtractionRegex(string benchmark)
        {
            string regexStr = null;
            if (benchmark == "tcp-all-all")
            {
                //prefer the easier one.
                regexStr = @"(\d+)(?!.*\d)";
            }
            else if (benchmark == "ping")
            {
                regexStr = @"time=(-?(([1-9]\d*)|0)(\.*0*[1-9](0*[1-9])*)?)";
            }
            else if (benchmark == "igi")
            {
                regexStr = @"\w\w\w:\s+(-?(([1-9]\d*)|0)(\.*[0-9]([0-9])*)?) Mpbs \(suggested\)";
            }
            else if (benchmark == "dpdk-echo")
            {
                regexStr = @"[\d+\.]+,[\d+\.]+,(\d+)";
            }
            return new Regex(regexStr);
        }
        //static Dictionary<string, int> GetId2Rank(string[] contents)
        //{
        //    Dictionary<string, int> id2Rank = new Dictionary<string, int>();
        //    var idRegex = IdentityDiscoveryRegex(InferBenchmark(contents));
        //    for (int i = 0; i < contents.Length; i++)
        //    {
        //        var currLine = contents[i];
        //        var match = idRegex.Match(currLine);
        //        if (match.Success)
        //        {
        //            for (int g = 1; g < match.Groups.Count; g++)
        //            {
        //                if (id2Rank.ContainsKey(match.Groups[g].Value) == false)
        //                {
        //                    id2Rank.Add(match.Groups[g].Value, id2Rank.Count);
        //                }
        //            }
        //        }
        //    }
        //    return id2Rank;
        //}

        static void PreProcess(List<double>[,] data, int drop)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    var total = data[i, j].Count;
                    var dropCnt = drop / 100.0 * total;
                    var dropHead = (int)(dropCnt / 2);
                    if (dropHead <= data[i, j].Count)
                    {
                        data[i, j].RemoveRange(0, dropHead);
                    }
                    if (dropHead <= data[i, j].Count)
                    {
                        data[i, j].RemoveRange(data[i, j].Count - dropHead, dropHead);
                    }
                }
            }
        }
        static void WriteCSVForPlots(List<double>[,] data, string file)
        {
            //for each file write to a subfolder.
            var fi = new FileInfo(file);
            var fldr = string.Format("{0}/{1}_FLDR", fi.DirectoryName, fi.Name);
            if (Directory.Exists(fldr))
            {
                Directory.Delete(fldr, true);
            }
            var di = Directory.CreateDirectory(fldr);
            var desc = string.Format("{0}/metadata", di.FullName);
            var max = 0;
            var maxY = 0.0;
            var minY = double.MaxValue;
            for (int from = 0; from < data.GetLength(0); from++)
            {
                for (int to = 0; to < data.GetLength(0); to++)
                {
                    var lst = data[from, to];
                    var fName = string.Format("{2}/{0}-{1}", from, to, di.FullName);
                    if (from != to)
                    {
                        max = Math.Max(max, lst.Count);
                        maxY = Math.Max(maxY, lst.Count == 0 ? 0 : lst.Max());
                        minY = Math.Min(minY, lst.Count == 0 ? double.MaxValue : lst.Min());
                    }
                    using (var sw = new StreamWriter(fName))
                    {
                        for (int i = 0; i < lst.Count; i++)
                        {
                            sw.WriteLine(string.Format("{0},{1}", i, lst[i]));
                        }
                    }
                }
            }

            using (var sw = new StreamWriter(desc))
            {
                sw.WriteLine(string.Format("MAX_X:{0};MAX_Y:{1};MIN_Y:{2};DIM:{3}", max, maxY, minY, data.GetLength(0)));
            }
        }
        static ProgressBar prog = new ProgressBar();
        static Dictionary<string, int> host2Rank = new Dictionary<string, int>();
        const int CONCURRENCY_BLOCK_SIZE = 512;
        //static HashSet<string> unknownIds = new HashSet<string>();

        //remember, system.collection.list can be read by multiple threads simultaneously.
        static void Dispatch(List<string> contents, Regex idReg, List<double>[,] data, Regex valReg)
        {
            Parallel.ForEach(contents, currLine =>
            {
                var from = "NON_EXISTENT";
                var to = "NON_EXISTENT";
                var idMch = idReg.Matches(currLine);
                if (idMch.Count != 0)
                {
                    from = idMch[0].Groups[1].Value;
                    if (idMch[0].Groups.Count >= 2)
                    {
                        to = idMch[0].Groups[2].Value;
                    }
                }
                if (host2Rank.ContainsKey(from) == false)
                {
                    Console.WriteLine("Warning: From={0} is unknown. Skipping {1}", from, currLine);
                    return;
                }
                if (host2Rank.ContainsKey(to) == false)
                {
                    Console.WriteLine("Warning: To={0} is unknown. Skipping {1}", to, currLine);
                    return;
                }
                var fromId = host2Rank[from];
                var toId = host2Rank[to];
                var lst = data[fromId, toId];
                var valMch = valReg.Match(currLine);
                if (valMch.Success)
                {
                    var val = double.Parse(valMch.Groups[1].Value);
                    if (val < 0)
                    {
                        Console.WriteLine("check engine! val={0}, line={1}", val, currLine);
                        Debug.Assert(false);
                    }
                    if (val == 0)
                    {
                        Console.WriteLine("[warning] 0 observed from {0} to {1}. Faulty line = {2}", hosts[fromId], hosts[toId], currLine);
                    }
                    lock (lst)
                    {
                        lst.Add(val);
                    }
                }
            });
        }

        static void Process(string file)
        {
            var sr = new StreamReader(file);
            var content = sr.ReadLine();
            //var lines = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //Interlocked.Add(ref totalLines, lines.Length);
            var benchmark = InferBenchmark(content);
            var selfTestOption = SelfTestOption(content);
            var dim = Math.Min(GetDimension(content), host2Rank.Count);
            var valueExp = ValueExtractionExp(content);
            //var regex4QPerf = new Regex(@"latency\s*= \s*(-?(([1-9]\d*)|0)(.*0*[1-9](0*[1-9])*)?) us", RegexOptions.Compiled);
            var data = new List<double>[dim, dim];
            #region INITDATA
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    data[i, j] = new List<double>();
                }
            }
            #endregion
            //var singleNodeCosts = new double[dim];
            var id2Rank = host2Rank;// GetId2Rank(lines);
            //Debug.Assert(id2Rank.Count == dim);
            var idReg = IdentityDiscoveryRegex(benchmark);
            var valReg = ValueExtractionRegex(benchmark);

            //skip control line.

            List<string> contents = new List<string>();

            while (sr.EndOfStream == false)
            {
                var currLine = sr.ReadLine();
                //can we extract anything?
                //prioritize id extraction over value extraction.
                contents.Add(currLine);
                if (sr.EndOfStream || (contents.Count > 0 && contents.Count % CONCURRENCY_BLOCK_SIZE == 0))
                {
                    Dispatch(contents, idReg, data, valReg);
                }
            }

            PreProcess(data, PreprocessOptions(content));
            WriteProcessedMatrix(data, selfTestOption, valueExp, file);
            WriteCSVForPlots(data, file);
        }

        static List<string> hosts = new List<string>();

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("dotnet run file-to-analyze host-file-path");
            }
            //var fp = args.Length > 0 ? args[0] : "../benchmarks";
            var file = args[0];
            //Console.WriteLine("Searching for {0} in folder {1}", searchPattern, fp);
            var hostFile = args[1];

            using (var sr = new StreamReader(hostFile))
            {
                foreach (var line in sr.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    host2Rank.Add(line, host2Rank.Count);
                    hosts.Add(line);
                }
            }
            //procLines = files.Count();
            Process(file);
        }
    }
}
