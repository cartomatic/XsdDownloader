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
        public static List<String> Processed = new List<String>();
        public static List<String> Imported = new List<string>();
        public static List<String> Included = new List<string>();

        static void Main(string[] args)
        {
            if (!CommandLine.Parser.Default.ParseArguments(args, Params))
                return;

            // Download & process XSDs
            if (String.IsNullOrEmpty(Params.OutputDirectory))
                Params.OutputDirectory = ".";
            if (!Directory.Exists(Params.OutputDirectory))
                Directory.CreateDirectory(Params.OutputDirectory);

            foreach (var inputFile in Params.InputFiles) {
                DownloadAndParse(inputFile);
            }

            // Generate batch file for xsd.exe
            var sb = new StringBuilder("xsd /c");

            foreach (var baseName in Params.InputFiles.Select(Path.GetFileName)) {
                AddToImported(baseName);
            }

            if (!String.IsNullOrEmpty(Params.Namespace))
                sb.Append(" /namespace:").Append(Params.Namespace);

            foreach (var imported in Imported) {
                sb.Append(" ").Append(imported);
            }

            File.WriteAllText(Params.OutputDirectory + @"\create_classes_from_xsd.bat", sb.ToString());
        }

        static void DownloadAndParse(String url)
        {
            // Skip already processed XSDs
            if (Processed.Contains(url)) {
                return;
            }

            Console.WriteLine("Downloading and parsing: " + url);
            // Initialize paths
            var uri = new Uri(url);
            var fileName = Params.OutputDirectory + @"\" + Path.GetFileName(uri.LocalPath);
            var baseUri = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
            XDocument doc;

            // Download and parse
            using (var client = new WebClient()) {
                client.DownloadFile(url, fileName);
                doc = XDocument.Load(fileName);
            }

            // Recursively download all <include>d schemas
            foreach (var schemaFile in doc.Descendants()
                                          .Where(e => e.Name.LocalName == "include")
                                          .Select(e => e.Attribute("schemaLocation").Value)) {
                DownloadAndParse(baseUri + schemaFile);
                Included.Add(schemaFile);
            }

            // Recursively download all <import>ed schemas and put them on the command line (since
            // xsd.exe expects <import> 
            foreach (var path in doc.Descendants()
                                    .Where(e => e.Name.LocalName == "import")
                                    .Select(e => e.Attribute("schemaLocation").Value)) {
                AddToImported(path);
                DownloadAndParse(path);
            }

            Processed.Add(url);
        }

        static void AddToImported(String filename)
        {
            if (!Imported.Contains(Path.GetFileName(filename)) && !Included.Contains(Path.GetFileName(filename)))
                Imported.Add(Path.GetFileName(filename));
        }
    }
}
