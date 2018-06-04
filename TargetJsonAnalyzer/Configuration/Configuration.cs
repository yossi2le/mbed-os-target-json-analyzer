/* mbed Microcontroller Library
 * Copyright (c) 2018 ARM Limited
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using log4net;
using System;
using System.IO;

namespace TargetJsonAnalyzer.Config {
/// <summary>
/// Singleton configurator class to store process configuration parametes
/// </summary>
public class Configuration {

    #region Public members
    /// <summary>
    /// Configuration singleton class to hold the running parameters.
    /// </summary>
    public static Configuration Instance = new Configuration();
    #endregion

    #region Private members
    /// <summary>
    ///input file name.
    /// </summary>
    private String _inputFileName = string.Empty;

    /// <summary>
    /// output folder name
    /// </summary>
    private String _outputFolderName = string.Empty;

    /// <summary>
    /// the expected output format. EXCEL, CSV, Sqlite ot ALL
    /// </summary>
    private String _outputType = string.Empty;

    /// <summary>
    /// log4net logger object
    /// </summary>
    private ILog _log;

    /// <summary>
    /// status of configuration class. true or false
    /// </summary>
    private bool _isInitialize;
    #endregion

    #region Public Properties
    /// <summary>
    /// Output property of input file name
    /// </summary>
    public System.String InputFileName {
        get { return _inputFileName; }
    }

    /// <summary>
    ///Output property of output folder name
    /// </summary>
    public System.String OutputFolderName {
        get { return _outputFolderName; }
    }

    /// <summary>
    ///Output property of output format
    /// </summary>
    public System.String OutputType {
        get { return _outputType; }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Intialize the configuration by parsing command lin earguments
    /// </summary>
    /// <param name="args">A list of argument from command line</param>
    /// <returns>True for success and false for error</returns>
    public bool Initialize(string[] args)
    {
        _log = LogManager.GetLogger("TargetJsonAnalyzer");

        if (_isInitialize) {
            return true;
        }

        bool status = ParseParameters(args);

        _isInitialize = true;
        return status;
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Constructor.
    /// </summary>
    protected Configuration() { }
    #endregion

    #region Private Methods
    /// <summary>
    /// Parsing command lin earguments
    /// </summary>
    /// <param name="args">A list of argument from command line</param>
    /// <returns>True for success and false for error</returns>
    private bool ParseParameters(string[] args)
    {
        bool foundF = false;
        bool foundO = false;
        bool foundT = false;

        foreach (string arg in args) {
            string currentArgument = arg.Substring(0, 3);

            switch (currentArgument) {
                case "-f=":
                    foundF = true;
                    _inputFileName = Path.GetFullPath(arg.Substring(3, arg.Length - 3));
                    break;
                case "-o=":
                    foundO = true;
                    string dirName = arg.Substring(3, arg.Length - 3);
                    if (!dirName.EndsWith("\\") && !dirName.EndsWith("/")) {
                        dirName += "/";
                    }
                    _outputFolderName = Path.GetDirectoryName(dirName);
                    break;
                case "-t=":
                    foundT = true;
                    _outputType = arg.Substring(3, arg.Length - 3);
                    if (_outputType != "EXCEL" && _outputType != "CSV" && _outputType != "Sqlite" && _outputType != "ALL") {
                        //_log.Error("Unknown format type argument");
                        Console.WriteLine("Unknown format type argument");
                        PrintUsage();
                        return false;
                    }
                    break;
                default:
                    //_log.Error("Unkonwn command line argument " + currentArgument.Substring(0, 2));
                    Console.WriteLine("Unkonwn command line argument " + currentArgument.Substring(0, 2));
                    PrintUsage();
                    return false;
            }
        }
        if (!foundF) {
            //_log.Error("Missing command line argument -f");
            Console.WriteLine("Missing command line argument -f");
            PrintUsage();
            return false;
        }

        if (!foundO) {
            //_log.Error("Missing command line argument -o");
            Console.WriteLine("Missing command line argument -o");
            PrintUsage();
            return false;
        }

        if (!foundT) {
            //_log.Error("Missing command line argument -t");
            Console.WriteLine("Missing command line argument -t");
            PrintUsage();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prints usage
    /// </summary>
    private void PrintUsage()
    {
        Console.WriteLine("**************************************************************************************************");
        Console.WriteLine("*TargetJsonAnalyzer Usage:                                                                       *");
        Console.WriteLine("*                                                                                                *");
        Console.WriteLine("*TargetJsonAnalyzer -f=<path + file name> -o=<path>  -t <format type>                            *");
        Console.WriteLine("*                                                                                                *");
        Console.WriteLine("*    -f=<path + file name>           : The full path to the where the target.json                *");
        Console.WriteLine("*                                      file location.                                            *");
        Console.WriteLine("*    -o=<path>                       : Path to the output folder                                 *");
        Console.WriteLine("*    -t=<format type>                : specify the requested output format. support EXCEL,       *");
        Console.WriteLine("*                                      CSV or Sqlite                                             *");
        Console.WriteLine("**************************************************************************************************");
    }
    #endregion

}
}
