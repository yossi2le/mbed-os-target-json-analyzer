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
using TargetJsonAnalyzer.Config;
using TargetJsonAnalyzer.Interfaces.Dispatchers;

namespace TargetJsonAnalyzer.Dispatchers {

/// <summary>
/// Excel output dispatcher class
/// </summary>
class ExcelDispatcher : IExcelDispatcher {

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
            throw new Exception("Excel Dispatcher is not initialize");
        }

        ExportDataSetToExcel(_ds);
    }

    /// <summary>
    /// Export the dt table into the output file in a Excel format
    /// </summary>
    /// <param name="ds">The data set collection of all tables that need to be written to the output file</param>
    private void ExportDataSetToExcel(DataSet ds)
    {
        Microsoft.Office.Interop.Excel.Application excelApp = null;
        Microsoft.Office.Interop.Excel.Workbook excelWorkBook = null;
        var app = new Microsoft.Office.Interop.Excel.Application();
        var wb = app.Workbooks.Add();
        var filename = Configuration.Instance.OutputFolderName + "\\" +
                       Path.GetFileNameWithoutExtension(Configuration.Instance.InputFileName) + ".xlsx";

        try {
            wb.SaveAs(filename);
            wb.Close();

            _log.Info("Exporting data to Excel. This can take few minutes ");

            //Creae an Excel application instance
            excelApp = new Microsoft.Office.Interop.Excel.Application();

            //Create an Excel workbook instance and open it from the predefined location
            excelWorkBook = excelApp.Workbooks.Open(filename);

            int counter = 0;
            foreach (DataTable table in ds.Tables) {
                //Add a new worksheet to workbook with the Datatable name
                Microsoft.Office.Interop.Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                excelWorkSheet.Name = table.TableName;

                for (int i = 1; i < table.Columns.Count + 1; i++) {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                    counter++;
                    if (counter % 300 == 0) {
                        System.Console.Write(".");
                    }
                }

                for (int j = 0; j < table.Rows.Count; j++) {
                    for (int k = 0; k < table.Columns.Count; k++) {
                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                        counter++;
                        if (counter % 300 == 0) {
                            System.Console.Write(".");
                        }
                    }
                }
            }
        }
        finally {
            excelWorkBook.Save();
            excelWorkBook.Close();
            excelApp.Quit();
            System.Console.Write("\n");
        }

    }
}
}
