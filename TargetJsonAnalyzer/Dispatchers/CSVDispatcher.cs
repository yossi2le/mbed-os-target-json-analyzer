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
using System.Data;
using System.IO;
using System.Text;
using TargetJsonAnalyzer.Config;
using TargetJsonAnalyzer.Interfaces.Dispatchers;

namespace TargetJsonAnalyzer.Dispatchers {

/// <summary>
/// CSV output dispatcher class
/// </summary>
class CSVDispatcher : ICSVDispatcher {

    /// <summary>
    /// Data set collection of all the tables to be output
    /// </summary>
    private DataSet _ds = null;

    /// <summary>
    /// Log4net logger object
    /// </summary>
    private ILog _log = LogManager.GetLogger("TargetJsonAnalyzer");

    /// <summary>
    /// indicates if the dispatcher has been initialize
    /// </summary>
    private Boolean _isInitializa = false;

    /// <summary>
    /// Initialize the dispatcher
    /// </summary>
    /// <param name="ds">input data set collection of the tables to be output</param>
    public void Init( DataSet ds)
    {
        _ds = ds;
        _isInitializa = true;
    }

    /// <summary>
    /// The Dispatch generic method which send all table to the output file
    /// </summary>
    public void Dispatch()
    {
        if (!_isInitializa) {
            throw new Exception("CSV Dispatcher is not initialize");
        }

        foreach (DataTable table in _ds.Tables) {
            ExportDataToCsvFle(table);
        }
    }

    /// <summary>
    /// Export the dt table into the output file in a CSV format
    /// </summary>
    /// <param name="dt">The table to be witten to output file</param>
    public void ExportDataToCsvFle(DataTable dt)
    {
        StringBuilder str = new StringBuilder();
        int intColumnCount = dt.Columns.Count;
        int index = 1;
        //add column names
        foreach (DataColumn item in dt.Columns) {
            str.Append(String.Format("\"{0}\"", item.ColumnName));
            if (index < intColumnCount) {
                str.Append(";");
            } else {
                str.Append("\n");
            }
            index++;
        }

        foreach (DataRow dr in dt.Rows) {
            foreach (object field in dr.ItemArray) {
                str.Append(field.ToString() + ";");
            }
            str.Replace(";", "\n", str.Length - 1, 1);
        }

        try {
            var filename = Configuration.Instance.OutputFolderName + "\\" + Path.GetFileNameWithoutExtension(
                               Configuration.Instance.InputFileName) + "." + dt.TableName + ".csv";
            _log.Info("Exporting data to " + filename);
            File.WriteAllText(filename, str.ToString());
        } catch (Exception ex) {
            _log.Error("Write error. message:" + ex.Message);
            throw ex;
        }
    }
}
}
