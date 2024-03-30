using System;
using System.Reflection;
using PRISM;

namespace SqlFileSplitter
{
    public class SqlFileSplitterOptions
    {
        // Ignore Spelling: app, sql

        /// <summary>
        /// Program date
        /// </summary>
        public const string PROGRAM_DATE = "March 29, 2024";

        [Option("Input", "I", ArgPosition = 1, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "SQL script file to process")]
        public string InputFilePath { get; set; }

        [Option("LinesPerFile", "Lines", "L", HelpShowsDefault = false,
            HelpText = "Target number of lines to write to each output file")]
        public int LinesPerFile { get; set; } = 500000;

        [Option("MaximumOutputFiles", "MaxFiles", "M", HelpShowsDefault = false,
            HelpText = "Maximum number of output files to create; once the maximum has been reached, " +
                       "the remaining lines from the input file will be written to the final output file")]
        public int MaxOutputFiles { get; set; } = 25;

        [Option("Verbose", "V", HelpShowsDefault = true,
            HelpText = "When true, show additional status messages while processing the input file")]
        public bool VerboseOutput { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlFileSplitterOptions()
        {
            InputFilePath = string.Empty;
        }

        /// <summary>
        /// Get the program version
        /// </summary>
        public static string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version + " (" + PROGRAM_DATE + ")";
        }

        /// <summary>
        /// Show the options at the console
        /// </summary>
        public void OutputSetOptions()
        {
            Console.WriteLine("Options:");

            Console.WriteLine(" {0,-25} {1}", "Input script file:", PathUtils.CompactPathString(InputFilePath, 120));

            Console.WriteLine(" {0,-25} {1}", "Lines per file:", LinesPerFile);

            Console.WriteLine(" {0,-25} {1}", "Max output files:", MaxOutputFiles);

            Console.WriteLine(" {0,-25} {1}", "Verbose Output:", VerboseOutput);

            Console.WriteLine();
        }

        /// <summary>
        /// Validate the options
        /// </summary>
        /// <returns>True if options are valid, false if /I or /M is missing</returns>
        public bool ValidateArgs(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(InputFilePath))
            {
                errorMessage = "Use /I to specify the SQL script file to process";
                return false;
            }

            errorMessage = string.Empty;

            return true;
        }
    }
}
