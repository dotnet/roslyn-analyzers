// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AnalyzerCodeGenerator
{
    public class MessageTableGenerator
    {
        private static List<string> _xmlFiles;
        private static string _outputFile;

        private static List<CheckMessages> _checks = new List<CheckMessages>();

        public static void Main(string[] args)
        {
            var sourceDir = args[0]; 
            _outputFile = args[1];
            bool force = false;

            if (args.Length == 3 && args[2].Equals("/force")) { force = true; }

            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine("ERROR: directory provided for input XML files doesn't exist: " + sourceDir);
                return;
            }
            else
            {
                _xmlFiles = new List<string>(Directory.GetFiles(sourceDir).Where( file => file.EndsWith(".xml")));
                if (_xmlFiles.Count == 0)
                {
                    Console.WriteLine("ERROR: directory provided for input XML files doesn't have any xml file: " + sourceDir);
                    return;
                }
            }

            if (File.Exists(_outputFile)) {
                if (!force)
                {
                    Console.WriteLine("ERROR: can't overwrite existing output file: " + _outputFile);
                    Console.WriteLine("       pass /force on the command-line to delete and regenerate this content.");
                    return;
                }
                File.Delete(_outputFile);
            }

            foreach (var file in _xmlFiles)
            {
                _checks.AddRange(ParseCheckMessages(file));
            }

            File.WriteAllText(_outputFile, GenerateCsv(_checks));
        }

        private static IEnumerable<CheckMessages> ParseCheckMessages(string fileName)
        {                                   
            XDocument xmlFile = XDocument.Load(fileName);
            Debug.Assert(xmlFile.Root.Name == "Rules");
            foreach (var ruleElement in xmlFile.Root.Elements("Rule"))
            {
                var check = new CheckMessages();
                check.Messages = new Dictionary<string, string>();
                check.Id = ruleElement.Attribute("CheckId").Value;
                foreach (var resolutionElement in ruleElement.Elements("Resolution"))
                {
                    string resolutionName = resolutionElement.HasAttributes ? resolutionElement.Attribute("Name").Value : "Default";
                    check.Messages[resolutionName] = resolutionElement.Value;
                }
                yield return check;  
            }
            yield break;
        }

        private static string GenerateCsv(IEnumerable<CheckMessages> checks)
        {
            var csvHeader = new[] { "ID", "MessageName", "Message" };
            var sw = new StringWriter();
            using (var csv = new CsvWriter(sw))
            {

                // write header record
                foreach (var h in csvHeader)
                {
                    csv.WriteField(h);
                }
                csv.NextRecord();

                foreach (var check in checks)
                {
                    foreach (var message in check.Messages)
                    {
                        csv.WriteField(check.Id);
                        csv.WriteField(message.Key);
                        csv.WriteField(message.Value);
                        csv.NextRecord();
                    }
                }

                return sw.ToString();
            }
        }
    }
}
