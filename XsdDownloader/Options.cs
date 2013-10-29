using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace XsdDownloader {
    class Options {
        [OptionList('i', "input", ';', Required = true, HelpText = "URL of the root XSD file(s).")]
        public IList<String> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Directory where to save all downloaded files.")]
        public String OutputDirectory { get; set; }

        [Option('n', "namespace", HelpText = "Namespace for the generated class(es).")]
        public String Namespace { get; set; }

        [HelpOption]
        public String GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
