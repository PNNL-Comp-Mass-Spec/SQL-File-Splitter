# SQL File Splitter

This program processes a PostgreSQL DDL file with CREATE object commands,
COPY commands, INSERT INTO commands, etc. and splits the file into a series of 
output files, each with roughly the same number of lines. When the maximum number 
of files to create is reached, the remaining lines from the input file are 
written to the final output file. The program only switches to a new file
on a line with a CREATE object statement, COPY command, or INSERT INTO command.

## Console Switches

The SQL File Splitter is a console application, and must be run from the Windows command prompt.

```
SQLFileSplitter.exe
  /I:InputFilePath
  [/L:LinesPerFile]
  [/M:MaxFileCount]
  [/V]
  [/ParamFile:ParamFileName.conf] [/CreateParamFile]
```

* Use `/I` to specify the input file

* Use `/L`, `/Lines`, or `/LinesPerFile` to specify the target number of lines to write to each output file

* Use `/M`, `/MaxFiles`, or `/MaximumOutputFiles` to specify the maximum number of output files to create
  * Once the maximum has been reached, the remaining lines from the input file will be written to the final output file

* Use `/V` to show additional status messages while processing the input file

## Contacts

Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov\
Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
Source code: https://github.com/PNNL-Comp-Mass-Spec/PgSQL-Table-Creator-Helper

## License

Licensed under the 2-Clause BSD License; you may not use this program except
in compliance with the License.  You may obtain a copy of the License at
https://opensource.org/licenses/BSD-2-Clause

Copyright 2023 Battelle Memorial Institute
