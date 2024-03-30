using System;
using System.IO;
using System.Text.RegularExpressions;
using PRISM;

namespace SqlFileSplitter
{
    public class SqlFileSplitter : EventNotifier
    {
        // Ignore Spelling: Postgres, SQL

        private readonly SqlFileSplitterOptions mOptions;

        /// <summary>
        /// This RegEx matches the argument direction, name, type, and default value (if defined)
        /// </summary>
        private readonly Regex mCreateObjectMatcher = new("^CREATE (TABLE|VIEW|FUNCTION|PROCEDURE) [^ (]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Regex mLoadDataMatcher = new("^(COPY|INSERT INTO) [^ (]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private FileInfo mCurrentOutputFile;

        private int mCurrentOutputFileNumber;

        private StreamWriter mCurrentOutputWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Processing options</param>
        public SqlFileSplitter(SqlFileSplitterOptions options)
        {
            mOptions = options;
        }

        private void CreateNextOutputFile(FileSystemInfo inputFile, string baseOutputFilePath)
        {
            mCurrentOutputFileNumber++;

            mCurrentOutputFile = new FileInfo(string.Format("{0}{1:000}{2}", baseOutputFilePath, mCurrentOutputFileNumber, inputFile.Extension));

            if (mCurrentOutputFileNumber > 1)
            {
                mCurrentOutputWriter.Close();
            }

            Console.WriteLine("Writing data to {0}", mCurrentOutputFile.Name);

            mCurrentOutputWriter = new StreamWriter(new FileStream(mCurrentOutputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read));
        }

        /// <summary>
        /// Process the input file
        /// </summary>
        /// <returns>True if successful, false if an error</returns>
        public bool ProcessInputFile()
        {
            try
            {
                var inputFile = new FileInfo(mOptions.InputFilePath);

                if (!inputFile.Exists)
                {
                    OnErrorEvent("Input file not found: " + inputFile.FullName);
                    return false;
                }

                if (inputFile.DirectoryName is null)
                {
                    OnErrorEvent("Unable to determine the parent directory of the input file: " + inputFile.FullName);
                    return false;
                }

                var baseOutputFilePath = Path.Combine(
                    inputFile.DirectoryName,
                    Path.GetFileNameWithoutExtension(inputFile.Name) + "_part");

                CreateNextOutputFile(inputFile, baseOutputFilePath);
                var currentOutputLineCount = 0;

                Console.WriteLine();
                OnStatusEvent("Reading " + PathUtils.CompactPathString(inputFile.FullName, 100));
                OnStatusEvent("Writing " + PathUtils.CompactPathString(mCurrentOutputFile.FullName, 100));

                using var reader = new StreamReader(new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));

                var currentLineNumber = 0;

                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine() ?? string.Empty;
                    currentLineNumber++;
                    currentOutputLineCount++;

                    if (currentOutputLineCount >= mOptions.LinesPerFile &&
                        (mCreateObjectMatcher.IsMatch(dataLine) || mLoadDataMatcher.IsMatch(dataLine)))
                    {
                        if (mOptions.VerboseOutput)
                        {
                            Console.WriteLine(" Wrote {0:N0} lines", currentOutputLineCount);
                            Console.WriteLine();
                        }

                        // Create a new output file
                        CreateNextOutputFile(inputFile, baseOutputFilePath);
                        currentOutputLineCount = 1;
                    }
                    else if (mOptions.VerboseOutput)
                    {
                        var createObjectMatch = mCreateObjectMatcher.Match(dataLine);

                        if (createObjectMatch.Success)
                        {
                            ConsoleMsgUtils.ShowDebugCustom(createObjectMatch.Value, emptyLinesBeforeMessage: 0);
                        }
                        else
                        {
                            var loadObjectMatch = mLoadDataMatcher.Match(dataLine);

                            if (loadObjectMatch.Success)
                            {
                                ConsoleMsgUtils.ShowDebugCustom(loadObjectMatch.Value, emptyLinesBeforeMessage: 0);
                            }
                        }
                    }

                    mCurrentOutputWriter.WriteLine(dataLine);

                    if (mCurrentOutputFileNumber < mOptions.MaxOutputFiles)
                    {
                        continue;
                    }

                    ConsoleMsgUtils.ShowDebugCustom("Writing the remaining data from the input file to the current output file", emptyLinesBeforeMessage: 0);

                    // The maximum number of files to create has been reached
                    // Write the remaining lines to the current output file

                    var lastStatus = DateTime.UtcNow;

                    while (!reader.EndOfStream)
                    {
                        var dataLine2 = reader.ReadLine() ?? string.Empty;
                        currentLineNumber++;

                        mCurrentOutputWriter.WriteLine(dataLine2);

                        // ReSharper disable once InvertIf
                        if (currentLineNumber % 100000 == 0 && DateTime.UtcNow.Subtract(lastStatus).TotalSeconds >= 15)
                        {
                            ConsoleMsgUtils.ShowDebugCustom(string.Format("{0:N0} lines written", currentLineNumber), emptyLinesBeforeMessage: 0);
                            lastStatus = DateTime.UtcNow;
                        }
                    }
                }

                mCurrentOutputWriter.Close();

                Console.WriteLine();
                OnStatusEvent("Processed {0:N0} lines in the input file", currentLineNumber);

                return true;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error in ProcessInputFile", ex);
                return false;
            }
        }
    }
}
