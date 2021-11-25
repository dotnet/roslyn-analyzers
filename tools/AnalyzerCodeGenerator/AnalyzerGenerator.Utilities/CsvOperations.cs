// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;

namespace AnalyzerCodeGenerator
{
    public class CsvOperations
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
                Map(m => m.OriginalPriority).ConvertUsing<Priority>(row =>
                {
                    Priority ret;
                    try
                    {
                        ret = (Priority)Enum.Parse(typeof(Priority), row.GetField("Original Priority"));
                    }
                    catch (ArgumentException)
                    {
                        ret = Priority.None;
                    }
                    return ret;
                });
                Map(m => m.RevisedPriority).ConvertUsing<Priority>(row =>
                {
                    Priority ret;
                    try
                    {
                        ret = (Priority)Enum.Parse(typeof(Priority), row.GetField("Revised Priority"));
                    }
                    catch (ArgumentException)
                    {
                        ret = Priority.None;
                    }
                    return ret;
                });
            }
        }

        public static IEnumerable<CheckData> ParseCheckDatas(string csvFilePath)
        {
            string specPath = Path.GetFullPath(Path.Combine(csvFilePath));
            var csvConfig = new CsvConfiguration();
            csvConfig.RegisterClassMap<CheckDataMap>();
            var csv = new CsvReader(File.OpenText(specPath), csvConfig);
            return csv.GetRecords<CheckData>();
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
