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

using System;
using Newtonsoft.Json;
using System.Xml;
using Microsoft.Practices.Unity;
using log4net.Config;
using TargetJsonAnalyzer.Interfaces.Xml;
using TargetJsonAnalyzer.Xml;
using TargetJsonAnalyzer.Interfaces.Data;
using TargetJsonAnalyzer.Data;
using log4net;
using TargetJsonAnalyzer.Config;
using TargetJsonAnalyzer.Interfaces.Dispatchers;
using TargetJsonAnalyzer.Dispatchers;

namespace TargetJsonAnalyzer {

/// <summary>
/// TargetJsonAnalyzer is the main class of the Target Json Analyzer process.
/// </summary>
class TargetJsonAnalyzer {
    static IUnityContainer _unity;
    static ILog _log;

    /// <summary>
    ///Entery point for the Target Json Analyzer process. This is a static method which instantiate and configure
    ///the process then read target json into XML.
    /// </summary>
    /// <param name="args"> A list of arguments retrieve from command line. See usage at readme for more help.</param>
    static void Main(string[] args)
    {
        var analyzer = new TargetJsonAnalyzer();

        if (!analyzer.Configure(args)) {
            return;
        }

        String fileName = Configuration.Instance.InputFileName;

        _log.Info("Starting to analyze the file " + fileName);

        string fileContent = String.Empty;
        try {
            using (System.IO.StreamReader Reader = new System.IO.StreamReader(fileName)) {
                fileContent = Reader.ReadToEnd();
                if (fileContent != String.Empty) {
                    fileContent = fileContent.Replace(" [],", " null,");
                } else {
                    _log.Error("The file " + fileName + " is empty");
                    return;
                }
            }
        } catch (Exception ex) {
            _log.Error("Exception while reading the file " + fileName + " message: " + ex.Message);
            return;
        }

        try {
            XmlDocument xml = JsonConvert.DeserializeXmlNode(fileContent, "RootObject");

            XmlReader xr = new XmlNodeReader(xml);
            analyzer.Load(xr);
        } catch (Exception ex) {
            _log.Error("Exception" + fileName + " message: " + ex.Message);
        }

        _log.Info("Finished analyzing " + fileName);
    }

    /// <summary>
    /// This is the process configuration methos. Here we initialize the unity resolver,
    /// the logger and calling to the process configuration class to read all command line arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>true for success and fasle for error</returns>
    private bool Configure(string[] args)
    {
        try {
            XmlConfigurator.Configure();
            _unity = new UnityContainer();

            _log = LogManager.GetLogger("TargetJsonAnalyzer");

            _unity.RegisterType<IBoardTableXmlReader, BoardTableXmlReader>();
            _unity.RegisterType<IConfigTableXmlReader, ConfigTableXmlReader>();
            _unity.RegisterType<IPostBinaryHookTable, PostBinaryHookTable>();
            _unity.RegisterType<IOverridesTableXmlReader, OverridesTableXmlReader>();
            _unity.RegisterType<IExpectedSoftdevicesWithoffsetsTableXmlReader, ExpectedSoftdevicesWithoffsetsTableXmlReader>();
            _unity.RegisterType<IProgenTableXmlReader, ProgenTableXmlReader>();
            _unity.RegisterType<ITargetOverridesTableXmlReader, TargetOverridesTableXmlReader>();
            _unity.RegisterType<INormalizeData, NormalizeData>();
            _unity.RegisterType<ISqliteDispatcher, SqliteDispatcher>();
            _unity.RegisterType<ICSVDispatcher, CSVDispatcher>();
            _unity.RegisterType<IExcelDispatcher, ExcelDispatcher>();

            bool status = Configuration.Instance.Initialize(args);
            return status;
        } catch (Exception ex) {
            _log.Error("Fail to start due to configuration error. message:" + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Load method all data from XML into tables the dispatch all table to the correct dispatcher.
    /// </summary>
    /// <param name="xr">input XML file reader with all target.json data</param>
    private void Load(XmlReader xr)
    {
        var boardTableXmlReader = _unity.Resolve<IBoardTableXmlReader>();
        boardTableXmlReader.Load(xr, _unity);

        if (Configuration.Instance.OutputType.Equals("Sqlite") || Configuration.Instance.OutputType.Equals("ALL")) {
            var dispatcher = _unity.Resolve<ISqliteDispatcher>();
            dispatcher.Init(boardTableXmlReader.Ds);
            dispatcher.Dispatch();
        }

        if (Configuration.Instance.OutputType.Equals("EXCEL") || Configuration.Instance.OutputType.Equals("ALL")) {
            var dispatcher = _unity.Resolve<IExcelDispatcher>();
            dispatcher.Init(boardTableXmlReader.Ds);
            dispatcher.Dispatch();
        }

        if (Configuration.Instance.OutputType.Equals("CSV") || Configuration.Instance.OutputType.Equals("ALL")) {
            var dispatcher = _unity.Resolve<ICSVDispatcher>();
            dispatcher.Init(boardTableXmlReader.Ds);
            dispatcher.Dispatch();
        }
    }
}
}
