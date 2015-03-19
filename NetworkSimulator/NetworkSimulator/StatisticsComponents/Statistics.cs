using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using NetworkSimulator.SimulatorComponents;
using System.IO;

namespace NetworkSimulator.StatisticsComponents
{
    public class Statistics
    {
        public static string _MapName = Configuration.GetInstance().TopologyFilePath.ToLower().Replace("_map.dat", "").Replace("map\\", "");

        public static string _RoutingStrategyName = Configuration.GetInstance().UnicastRoutingAlgorithms.SingleOrDefault(a => a.Selected == true).Name;

        public static string _RequestTypeName { get; set; }

        private static bool WriteToExcel(string FileName, string SheetName, string Cell, string _Parameter)
        {
            if (!System.IO.File.Exists(FileName))
            {
                throw new System.IO.FileNotFoundException("File \"" + FileName + "\" could not be found!");
            }
            bool result = false;
            string ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=No;\"";
            string SQL = "UPDATE [" + SheetName + "$" + Cell + ":" + Cell + "] SET " + "F1" + "='" + _Parameter + "'";

            using (OleDbConnection Connection = new OleDbConnection(ConnectionString))
            {
                Connection.Open();
                using (OleDbCommand cmd = new OleDbCommand(SQL, Connection))
                {
                    int value = cmd.ExecuteNonQuery();
                    if (value > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private static void Choice(string FileName, string _Sheet, string _Cell, int _NumOfReqest, string _Parameter)
        {
            string _Row = "";
            switch (_NumOfReqest)
            {
                case 100:
                    _Row = "2";
                    break;

                case 200:
                    _Row = "3";
                    break;

                case 300:
                    _Row = "4";
                    break;

                case 400:
                    _Row = "5";
                    break;

                case 500:
                    _Row = "6";
                    break;

                case 600:
                    _Row = "7";
                    break;

                case 700:
                    _Row = "8";
                    break;

                case 800:
                    _Row = "9";
                    break;

                case 900:
                    _Row = "10";
                    break;

                case 1000:
                    _Row = "11";
                    break;

                case 1100:
                    _Row = "12";
                    break;

                case 1200:
                    _Row = "13";
                    break;

                case 1300:
                    _Row = "14";
                    break;

                case 1400:
                    _Row = "15";
                    break;

                case 1500:
                    _Row = "16";
                    break;

                case 1600:
                    _Row = "17";
                    break;

                case 1700:
                    _Row = "18";
                    break;

                case 1800:
                    _Row = "19";
                    break;

                case 1900:
                    _Row = "20";
                    break;

                case 2000:
                    _Row = "21";
                    break;

                default:
                    _Row = "22";
                    break;
            }

            WriteToExcel(FileName, _Sheet, _Cell + _Row, _Parameter);
        }

        public static void Ex(List<Response> _ResponsesForStatistics)
        {
            int[] arr = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };

            for (int i = 0; i < arr.Count(); i++)
            {
                List<Response> result = (from rs in _ResponsesForStatistics select rs)
                                                .Skip(0)
                                                .Take(arr[i])
                                                .ToList<Response>();

                double time = (from rs in _ResponsesForStatistics select rs.ComputingTime)
                                               .Skip(0)
                                               .Take(arr[i])
                                               .Sum();

                // compute num of request
                int parameter = result.Count(r => r.Path.Count > 0);
                int _NumOfRequest = result.Count;
                if (parameter == 0 && _NumOfRequest == 0) continue;

                // compute avg computing time
                double avgTime = time / arr[i];

                ExportExcel(@"ReportAccepted.xlsx", parameter.ToString(), _NumOfRequest);
                ExportExcel(@"ReportComputingTime.xlsx", avgTime.ToString(), _NumOfRequest);

                result.Clear();
                parameter = 0;
                _NumOfRequest = 0;
                time = 0;
                avgTime = 0;
            }
        }

        public static void WriteResultToText(List<Response> _ResponsesForStatistics)
        {
            Configuration cfg = Configuration.GetInstance();
            string resultText = cfg.getSelectedUnicastAlgorithm().Name;
            string computingResultText = cfg.getSelectedUnicastAlgorithm().Name;
            //string acceptedBandwidthRatioResultText = cfg.getSelectedUnicastAlgorithm().Name;
            List<int> splits = new List<int>();
            int splitNum = cfg.NumberOfSplit;
            int numsRequest = _ResponsesForStatistics.Count();
            int counter = 0;
            while (counter <= numsRequest)
            {
                counter += splitNum;
                if (counter <= numsRequest)
                {
                    splits.Add(counter);
                }
                else
                {
                    if (splits.Count > 0)
                    {
                        if (splits.ElementAt(splits.Count - 1) < numsRequest)
                            splits.Add(numsRequest);
                    }
                    else
                        splits.Add(numsRequest);
                }
            }


            foreach (int i in splits)
            {
                List<Response> result = (from rs in _ResponsesForStatistics select rs)
                                                .Skip(0)
                                                .Take(i)
                                                .ToList<Response>();
                /*double time = (from rs in _ResponsesForStatistics select rs.ComputingTime)
                                              .Skip(0)
                                              .Take(i)
                                              .Sum();*/

                double time = result.Select(item => item.ComputingTime).Sum();
                double totalBandwidth = result.Select(item => item.Request.Demand).Sum();
                double totalBandwidthAccepted = result.Where(item => item.Path.Count > 0).Select(item => item.Request.Demand).Sum();

                int numAccepted = result.Count(r => r.Path.Count > 0);
                double computingTime = time / i;
                double acceptedBandwidthRatio = totalBandwidthAccepted / totalBandwidth * 100;

                resultText += "\t" + numAccepted;
                computingResultText += "\t" + computingTime;
                //acceptedBandwidthRatioResultText += "\t" + acceptedBandwidthRatio;
            }

            // check for directory exist
            string[] dir = cfg.StatisticsFilepath.Split('\\');
            string directory = "";
            for (int i = 0; i < dir.Count() - 1; i++)
            {
                directory += dir[i] + "\\";
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            /*string[] dir = cfg.StatisticsFilepath.Split('\\');
            string fileRequestName = dir[dir.Length - 1];
            if (!Directory.Exists(cfg.StatisticsFilepath))
            {
                Directory.CreateDirectory(cfg.StatisticsFilepath);
            }*/

            // if result file not exist create the split info and create a new file
            string resultfile = cfg.StatisticsFilepath + "." + cfg.ResultPrefix;
            //string resultfile = string.Format("{0}\\{2}_[{1}].{2}", cfg.StatisticsFilepath, fileRequestName, cfg.ResultPrefix);
            if (!File.Exists(resultfile))
            {
                string info = "#";
                foreach (int i in splits)
                {
                    info += "\t" + i;
                }
                FileStream fileResult = new FileStream(resultfile, FileMode.Create);
                StreamWriter wr = new StreamWriter(fileResult);
                wr.WriteLine(info);
                wr.Dispose();
                wr.Close();
                fileResult.Dispose();
                fileResult.Close();
            }

            // Append to text file
            FileStream f = new FileStream(resultfile, FileMode.Append);
            StreamWriter wrc = new StreamWriter(f);
            wrc.WriteLine(resultText);
            wrc.Close();
            f.Close();

            // if result file not exist create the split info and create a new file
            string computingFile = cfg.StatisticsFilepath + "." + cfg.ComputingPrefix;
            //string computingFile = string.Format("{0}\\{2}_[{1}].{2}", cfg.StatisticsFilepath, fileRequestName, cfg.ComputingPrefix);
            if (!File.Exists(computingFile))
            {
                string info = "#";
                foreach (int i in splits)
                {
                    info += "\t" + i;
                }
                FileStream fileResult = new FileStream(computingFile, FileMode.Create);
                StreamWriter wr = new StreamWriter(fileResult);
                wr.WriteLine(info);
                wr.Dispose();
                wr.Close();
                fileResult.Dispose();
                fileResult.Close();
            }

            // Append to text file
            FileStream cf = new FileStream(computingFile, FileMode.Append);
            StreamWriter wrcf = new StreamWriter(cf);
            wrcf.WriteLine(computingResultText);
            wrcf.Close();
            cf.Close();

            /*
            // if result file not exist create the split info and create a new file
            string acceptedBandwidthRatioFile = cfg.StatisticsFilepath + "." + cfg.AcceptedBandwidthRatioPrefix;
            //string acceptedBandwidthRatioFile = string.Format("{0}\\{2}_[{1}].{2}", cfg.StatisticsFilepath, fileRequestName, cfg.AcceptedBandwidthRatioPrefix);
            if (!File.Exists(acceptedBandwidthRatioFile))
            {
                string info = "#";
                foreach (int i in splits)
                {
                    info += "\t" + i;
                }
                FileStream fileResult = new FileStream(acceptedBandwidthRatioFile, FileMode.Create);
                StreamWriter wr = new StreamWriter(fileResult);
                wr.WriteLine(info);
                wr.Dispose();
                wr.Close();
                fileResult.Dispose();
                fileResult.Close();
            }

            // Append to text file
            FileStream af = new FileStream(acceptedBandwidthRatioFile, FileMode.Append);
            StreamWriter wrca = new StreamWriter(af);
            wrca.WriteLine(acceptedBandwidthRatioResultText);
            wrca.Close();
            af.Close();*/
        }

        public static void WriteStandardDeviationResultToText(List<Response> _ResponsesForStatistics, NetworkSimulator.NetworkComponents.Topology _Topology)
        {
            Configuration cfg = Configuration.GetInstance();
            string standardDeviationResultText = cfg.getSelectedUnicastAlgorithm().Name;
            List<int> splits = new List<int>();
            int splitNum = cfg.NumberOfSplit;//Bang nhau het
            int numsRequest = _ResponsesForStatistics.Count();
            int counter = 0;
            while (counter <= numsRequest)
            {
                counter += splitNum;
                if (counter <= numsRequest)
                {
                    splits.Add(counter);
                }
                else
                {
                    if (splits.Count > 0)
                    {
                        if (splits.ElementAt(splits.Count - 1) < numsRequest)
                            splits.Add(numsRequest);
                    }
                    else
                        splits.Add(numsRequest);
                }
            }


            foreach (int i in splits)
            {
                List<Response> result = (from rs in _ResponsesForStatistics select rs)
                                                .Skip(0)
                                                .Take(i)
                                                .ToList<Response>();
                Response _Response = null;
                if (result.Count > 0)
                    _Response = result.ElementAt(result.Count - 1);
                List<double> _Values = new List<double>();
                if (!Object.Equals(_Response, null))
                {
                    foreach (var _Link in _Topology.Links)
                    {
                        var _Victim = _Link.PercentOfBandwidthUsed.Where(item => Object.Equals(item.Key, _Response)).FirstOrDefault();
                        if (!Object.Equals(_Victim, null))
                            _Values.Add(_Victim.Value);
                    }
                }
                
                standardDeviationResultText += "\t" + NetworkSimulator.RoutingComponents.CommonAlgorithms.MathHelper.StandardDeviation(_Values);
            }

            // check for directory exist
            string[] dir = cfg.StatisticsFilepath.Split('\\');
            string directory = "";
            for (int i = 0; i < dir.Count() - 1; i++)
            {
                directory += dir[i] + "\\";
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // if result file not exist create the split info and create a new file
            string resultfile = cfg.StatisticsFilepath + "." + cfg.StandardDeviationPrefix;
            if (!File.Exists(resultfile))
            {
                string info = "#";
                foreach (int i in splits)
                {
                    info += "\t" + i;
                }
                FileStream fileResult = new FileStream(resultfile, FileMode.Create);
                StreamWriter wr = new StreamWriter(fileResult);
                wr.WriteLine(info);
                wr.Dispose();
                wr.Close();
                fileResult.Dispose();
                fileResult.Close();
            }

            // Append to text file
            FileStream f = new FileStream(resultfile, FileMode.Append);
            StreamWriter wrc = new StreamWriter(f);
            wrc.WriteLine(standardDeviationResultText);
            wrc.Close();
            f.Close();
        }

        public static void ExportExcel(string FileName, string _Parameter, int _NumOfRequest)
        {
            string _Cell = "";
            switch (_RoutingStrategyName)
            {
                case "MHA":
                    _Cell = "B";
                    break;
                case "WSP":
                    _Cell = "C";
                    break;
                case "BCRA":
                    _Cell = "D";
                    break;
                case "MIRA":
                    _Cell = "E";
                    break;
                case "DORA":
                    _Cell = "F";
                    break;
                case "RRATE":
                    _Cell = "G";
                    break;
                case "POOA":
                    _Cell = "H";
                    break;

                case "MCSRA":
                    _Cell = "I";
                    break;

                case "BGHT1":
                    _Cell = "J";
                    break;

                case "BGHT2":
                    _Cell = "K";
                    break;

                case "TEARD":
                    _Cell = "L";
                    break;

                case "RBA":
                    _Cell = "M";
                    break;

                case "PBMTA":
                    _Cell = "N";
                    break;

                default:
                    _Cell = "O";
                    break;
            }
            if (_MapName.Contains("mira"))
            {
                if (_RequestTypeName == "static")
                {
                    Choice(FileName, "MIRAStatic", _Cell, _NumOfRequest, _Parameter);
                }

                if (_RequestTypeName == "dynamic")
                {
                    Choice(FileName, "MIRADynamic", _Cell, _NumOfRequest, _Parameter);
                }

                if (_RequestTypeName == "mix")
                {
                    Choice(FileName, "MIRAMix", _Cell, _NumOfRequest, _Parameter);
                }
            }

            if (_MapName.Contains("cesnet"))
            {
                if (_RequestTypeName == "static")
                {
                    Choice(FileName, "CESNETStatic", _Cell, _NumOfRequest, _Parameter);
                }

                if (_RequestTypeName == "dynamic")
                {
                    Choice(FileName, "CESNETDynamic", _Cell, _NumOfRequest, _Parameter);
                }

                if (_RequestTypeName == "mix")
                {
                    Choice(FileName, "CESNETMix", _Cell, _NumOfRequest, _Parameter);
                }
            }
            if (_MapName.Contains("ansnet"))
            {
                if (_RequestTypeName == "static")
                {
                    Choice(FileName, "ANSNETStatic", _Cell, _NumOfRequest, _Parameter);
                }

                if (_RequestTypeName == "dynamic")
                {
                    Choice(FileName, "ANSNETDynamic", _Cell, _NumOfRequest, _Parameter);
                }

            }
        }
    }
}