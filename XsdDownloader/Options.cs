﻿using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace XsdDownloader {
    class Options {
        [OptionList('i', "input", ';', Required = true, HelpText = "URL(s) of the root XSD file(s).")]
        public IList<String> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Directory where to save all downloaded files.")]
        public String OutputDirectory { get; set; }

        [Option('n', "namespace", HelpText = "Namespace for the generated class(es).")]
        public String Namespace { get; set; }

        [Option('e', "xsd.exe", HelpText = "Path to XSD executable. Will not be called, used for generating create_classes_from_xsd.bat content.")]
        public String Executable { get; set; }

        [HelpOption]
        public String GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
