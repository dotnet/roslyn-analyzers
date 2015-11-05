using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Roslyn.Analyzers.SolutionGenerator
{
    internal static class Utilities
    {
        public static string GetGuid()
        {
            return "{" + Guid.NewGuid().ToString().ToUpperInvariant() + "}";
        }

        public static List<string> ConvertArrayToStringList(Array array)
        {
            var result = new List<string>(array.Length);

            for (int i = 1; i < array.Length; i++)
            {
                object item = array.GetValue(new int[] { i, 1 });

                if (item == null)
                    break; // we're done

                result.Add(item.ToString());
            }

            return result;
        }

        public static string ConvertStringToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException();
            }

            bool newWord = true;
            StringBuilder sb = new StringBuilder();
            foreach (var c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    newWord = true;
                    continue;
                }
                sb.Append(newWord ? char.ToUpper(c) : c);
                newWord = false;
            }

            return sb.ToString();
        }

        public static void CopyFile(string source, string target, string fileName)
        {
            File.Copy(source + @"\" + fileName, target + @"\" + fileName);
        }

        public static void CreateFile(string content, string target, string fileName)
        {
            File.WriteAllText(target + @"\" + fileName, content);
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
