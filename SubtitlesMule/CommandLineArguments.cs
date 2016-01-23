using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitlesMule
{
    class CommandLineArguments
    {
        [Option('r', "register", Required = false, HelpText = "Register File Types to process")]
        public string RegisterFileTypes { get; set; }

        [Option('f', "file", Required = true, HelpText = "Download subtitles for file")]
        public string File { get; set; }

        [Option('d', "directory", Required = false, HelpText = "Download subtitles for all files in directory (recursively)")]
        public string Directory { get; set; }

        [Option('l', "Language", Required = true, HelpText = "Language for subtitles use ISO 639-2/B Codes")]
        public string Language { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText();

            if (this.LastParserState.Errors.Any())
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
            return help;
        }
    }
}
