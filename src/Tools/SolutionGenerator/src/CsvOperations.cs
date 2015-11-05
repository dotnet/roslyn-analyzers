using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;

namespace Roslyn.Analyzers.SolutionGenerator
{
    internal class CsvOperations
    { 
        private sealed class CheckDataMap : CsvClassMap<CheckData>
        {
            public CheckDataMap()
            {
                Map(m => m.Id).Name("Id");
                Map(m => m.Name).Name("Name");
                Map(m => m.Title).Name("Title");
                Map(m => m.Description).Name("Description");
                Map(m => m.AnalyzerProject).Name("Proposed Analyzer");
                Map(m => m.Category).Name("Category");
                Map(m => m.Port).ConvertUsing<PortStatus>(row =>
                {
                    PortStatus ret;
                    try
                    {
                        ret = (PortStatus)Enum.Parse(typeof(PortStatus), row.GetField("Port?"));
                    }
                    catch (ArgumentException)
                    {
                        ret = PortStatus.None;
                    }
                    return ret;
                });
                //Map(m => m.Port).Name("Port?");
            }
        }

        public static IEnumerable<CheckData> ParseCheckDatas(string csvFilePath)
        {
            string specPath = Path.GetFullPath(Path.Combine(csvFilePath));
            var csvConfig = new CsvConfiguration();
            csvConfig.RegisterClassMap<CheckDataMap>();
            var csv = new CsvReader(File.OpenText(specPath), csvConfig);
            // todo: only emit for 'Yes' when spreadsheet is filled-up
            return csv.GetRecords<CheckData>().Where(c => c.Port == PortStatus.Yes || c.Port == PortStatus.Ported);
        }

        // this method will populate Message property for each CheckData
        public static void ParseCheckMessages(string csvFilePath, Dictionary<string, CheckData> checks)
        {
            string specPath = Path.GetFullPath(Path.Combine(csvFilePath)); 
            var csv = new CsvReader(File.OpenText(specPath));
            while (csv.Read())
            {
                var id = csv.GetField("ID");
                var messageName = csv.GetField("MessageName");
                var message = csv.GetField("Message");

                CheckData check;
                if (checks.TryGetValue(id, out check))
                {
                    if (check.Messages == null)
                    {
                        check.Messages = new Dictionary<string, string>();
                    }
                    check.Messages[Utilities.ConvertStringToPascalCase(messageName)] = message;
                }
            }
        }
    }
}
