using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace XsdDownloader {
    class Program {
        public static Options Params = new Options();
        public enum DownloadType {
            Direct,
            Include,
            Import
        }

        static void Main(string[] args)
        {
            if (!CommandLine.Parser.Default.ParseArguments(args, Params))
                return;

            // Download & process XSDs
            if (String.IsNullOrEmpty(Params.OutputDirectory))
                Params.OutputDirectory = ".";

            var toGet = Params.InputFiles.ToDictionary(s => s, s => DownloadType.Direct);
            var gotten = new HashSet<String>();
            var toBat = new HashSet<String>();

            while (toGet.Count > 0) {
                var task = toGet.First();
                var uri = new Uri(task.Key);

                Console.WriteLine("{0,7}: {1}", task.Value.ToString().ToUpperInvariant(), uri);

                // Set paths
                var file = Path.GetFullPath(Params.OutputDirectory + @"\" + uri.Host + @"\" + uri.LocalPath);
                var folder = Path.GetDirectoryName(file);
                var remoteFolder = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length - 1);

                // Download the file
                Directory.CreateDirectory(folder);
                using (var client = new WebClient()) {
                    client.DownloadFile(uri, file);
                }

                // Add imports to .bat command
                if (task.Value == DownloadType.Import || task.Value == DownloadType.Direct)
                    toBat.Add(file);

                var doc = XDocument.Load(file);

                // Find and queue includes and imports
                foreach (var element in doc.Descendants()
                              .Where(e => e.Name.LocalName == "include" || e.Name.LocalName == "import"))
                {
                    var schemaFile = element.Attribute("schemaLocation").Value;
                    var type = DownloadType.Import;
                    if (element.Name.LocalName == "include") {
                        schemaFile = remoteFolder + "/" + schemaFile;
                        type = DownloadType.Include;
                    }
                    if (!toGet.ContainsKey(schemaFile) && !gotten.Contains(schemaFile))
                        toGet.Add(schemaFile, type);
                }

                gotten.Add(task.Key);
                toGet.Remove(task.Key);
            }

            // Generate batch 
            if (String.IsNullOrEmpty(Params.Executable))
                Params.Executable = "xsd";
            var sb = new StringBuilder("\"").Append(Params.Executable).Append("\" /c");

            if (!String.IsNullOrEmpty(Params.Namespace))
                sb.Append(" /namespace:").Append(Params.Namespace);

            foreach (var imported in toBat) {
                sb.Append(" ^\r\n \"").Append(imported).Append("\"");
            }

            File.WriteAllText(Params.OutputDirectory + @"\create_classes_from_xsd.bat", sb.ToString());
        }

    }
}
