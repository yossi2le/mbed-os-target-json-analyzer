#  TargetJsonAnalyzer
TargetJsonAnalyzer is a tool that’s analyze target.json configuration file for inheritance and normalize it's data.

## Description
Mbed-os has a target json configuration file which describe the features and characteristic of each supported board, however the configuration rules together with the inheritance mechanism can make it difficult to figure the complete feature, parameter and characteristic of a specific board. That’s is specifically true if the board has a multiple inheritance or a long line of inheritance.
let’s assume the following example.

```
    E								
   / \
  D   C
  |
  B
  |
  A
```

If we like to know the what are the features of our E board we shall search the entire inheritance tree and figure out what is the final outcome. This process is prone for mistakes. 
The TargetJsonAnalyer comes to normalize the data for every board and flatten the inheritance so the result shows every board or device the exact features and characters. The output of the TargetJsonAnalyer process is either one of the following: Excel, CSV(semicolon) or SQLite database.

The following tables will be generated.
1. Boards: A list of all the boards. The data is row data, with no normalization or inheritance, and without the following fields which has special tables of their own.
	1. config
	2. post_binary_hook
	3. overrides
	4. progen
	5. target_overrides
	6. EXPECTED_SOFTDEVICES_WITH_OFFSETS
2. Normalize_boards: This table hold a normalize data for each board and also flatten the entire inheritance of every board. The inheritance line can be find under the inheritance field. 
3. Boards_final: This table show only boards with public field true. Also the column of features are set in the following order: macros -> device_has -> feature -> extra_lables.
	
For a complete explanation regarding target.json please refer to the following link:https://os.mbed.com/docs/v5.6/tools/adding-and-configuring-targets.html

## Prerequisite
The tool is written in C# and therefore require windows machine with .net 4.5.2 or higher. 

## Usage
Run the TargetJsonAnalyzer.exe from command line. A usage help will be shown on the screen.

```
**************************************************************************************************
*TargetJsonAnalyzer Usage:                                                                       *
*                                                                                                *
*TargetJsonAnalyzer -f=<path + file name> -o=<path>  -t <format type>                            *
*                                                                                                *
*    -f=<path + file name>           : The full path to the where the target.json                *
*                                      file location.                                            *
*    -o=<path>                       : Path to the output folder                                 *
*    -t=<format type>                : specify the requested output format. support EXCEL,       *
*                                      CSV or Sqlite                                             *
**************************************************************************************************
```
1. Please remember that none of the parameter is optional.
2. A log is generated in the running folder.
3. EXCEL type of file needs an Excel application to be installed on the running machine 
4. In Case of spaces in the input or output path use "PATH"

Example:
```
TargetJsonAnalyzer.exe -f=C:/dev/device-key/mbed-os/targets/targets.json -o="C:/Users/someuser/Desktop" -t=EXCEL
```

TODO: 
- Add inheritance boards to Config, PostBinaryHook, Overrides, ExpectedSoftDevicesWithoffsets, Progen and TargetOverrides. currently they holds only raw data.


