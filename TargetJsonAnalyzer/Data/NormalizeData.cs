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
using TargetJsonAnalyzer.Interfaces.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;

namespace TargetJsonAnalyzer.Data {

/// <summary>
/// This class is responsibal efor normalization of the data in Normalized_Boards
/// table and Boards_Features
/// </summary>
class NormalizeData : INormalizeData {

    /// <summary>
    ///DataSet collection for the tables.
    /// </summary>
    private DataSet _ds = null;

    /// <summary>
    /// This is the normalize table.
    /// </summary>
    private DataTable normalized = null;

    /// <summary>
    /// Log4net logger object
    /// </summary>
    private ILog _log = LogManager.GetLogger("TargetJsonAnalyzer");

    /// <summary>
    /// Constructor
    /// </summary>
    public NormalizeData() {}

    /// <summary>
    /// Start Normalizing the entire data
    /// </summary>
    /// <param name="ds">Data set with all the parsed table</param>
    public void Start(DataSet ds)
    {
        _ds = ds;
        DataTable dt = _ds.Tables["Boards"];

        int index1 = 1;
        while (true) {
            var offspringRows = dt.Select("inx = '" + index1.ToString() + "'");

            if (offspringRows.Length == 0) {
                break;
            }

            var offspringRow = offspringRows[0];

            IList<DataRow> parentsRows = null;
            try {
                parentsRows = GetAllParents(offspringRow, dt);
                if (normalized == null) {
                    normalized = new DataTable("Normalized_Boards");
                    foreach (DataColumn col in dt.Columns) {
                        normalized.Columns.Add(col.ColumnName);
                    }
                }
            } catch (Exception ex) {
                _log.Error("Normalized data failed. message:" + ex.Message);
                throw ex;
            }

            var newRow = normalized.NewRow();
            parentsRows.Add(offspringRow);
            for (int i = 0; i < parentsRows.Count; i++) {
                var prow = parentsRows[i];
                foreach (DataColumn col in normalized.Columns) {
                    if (!col.ColumnName.Equals("device_has") && !col.ColumnName.Equals("device_has_add") &&
                            !col.ColumnName.Equals("device_has_remove") &&
                            !col.ColumnName.Equals("macros") && !col.ColumnName.Equals("macros_add") && !col.ColumnName.Equals("macros_remove") &&
                            !col.ColumnName.Equals("features") && !col.ColumnName.Equals("features_add") &&
                            !col.ColumnName.Equals("features_remove") &&
                            !col.ColumnName.Equals("extra_labels") && !col.ColumnName.Equals("extra_labels_add") &&
                            !col.ColumnName.Equals("extra_labels_remove") &&
                            !col.ColumnName.Equals("inherits")) {
                        if (prow[col.ColumnName].ToString().Equals("null")) {
                            newRow[col.ColumnName] = String.Empty;
                        } else if (!String.IsNullOrEmpty(prow[col.ColumnName].ToString()) || col.ColumnName.Equals("public")) {
                            newRow[col.ColumnName] = prow[col.ColumnName];
                        }
                    } else {
                        if (String.IsNullOrEmpty(prow[col.ColumnName].ToString()) && !col.ColumnName.Equals("inherits")) {
                            continue;
                        }
                        if (prow[col.ColumnName].ToString().Equals("null")) {
                            newRow[col.ColumnName] = String.Empty;
                            continue;
                        }

                        var tempString = newRow[col.ColumnName].ToString();
                        if (col.ColumnName.Equals("inherits")) {
                            if (parentsRows.Count > 1 && i != parentsRows.Count - 1) {
                                tempString += "," + prow["BoardName"];
                            }
                        } else {
                            tempString += "," + prow[col.ColumnName];
                        }

                        newRow[col.ColumnName] = tempString.TrimStart(',');
                    }
                }
            }

            try {
                //fix add remove
                foreach (DataColumn col in normalized.Columns) {
                    String mainColName = String.Empty;
                    if (!(col.ColumnName.Contains("_add") || col.ColumnName.Contains("_remove"))) {
                        continue;
                    }

                    if (String.IsNullOrEmpty(newRow[col.ColumnName].ToString())) {
                        continue;
                    }

                    mainColName = col.ColumnName.Replace("_remove", "").Replace("_add", "");

                    if (col.ColumnName.Contains("_add")) {
                        newRow[mainColName] = newRow[mainColName].ToString() + "," + newRow[col.ColumnName].ToString();
                        newRow[mainColName] = newRow[mainColName].ToString().TrimStart(',');
                    }

                    if (col.ColumnName.Contains("_remove")) {
                        var removeList = newRow[col.ColumnName].ToString().Split(',');
                        foreach (var removeItem in removeList) {
                            newRow[mainColName] = newRow[mainColName].ToString().Replace(removeItem + ",", "");
                            newRow[mainColName] = newRow[mainColName].ToString().Replace(removeItem + " ,", "");
                            newRow[mainColName] = newRow[mainColName].ToString().Replace("," + removeItem, "");
                            newRow[mainColName] = newRow[mainColName].ToString().Replace(", " + removeItem, "");
                        }
                    }
                }

                normalized.Rows.Add(newRow);
            } catch (Exception ex) {
                _log.Error("Normalized data failed. message:" + ex.Message);
                throw ex;
            }

            index1++;
        }

        foreach (DataColumn col in dt.Columns) {
            if (!(col.ColumnName.Contains("_add") || col.ColumnName.Contains("_remove"))) {
                continue;
            }

            normalized.Columns.Remove(col.ColumnName);
        }

        _ds.Tables.Add(normalized);

        AddAllContcatedColumns(normalized, "device_has");
        AddAllContcatedColumns(normalized, "macros");
        AddAllContcatedColumns(normalized, "features");
        AddAllContcatedColumns(normalized, "extra_labels");

        //Create our final table, flat, only public and no concatanate fields.
        //We need only device_has columns in this table
        FinlizeTable(normalized);
    }

    /// <summary>
    /// Create a final table for board features
    /// </summary>
    /// <param name="dt">The normalize data table object</param>
    private void FinlizeTable(DataTable dt)
    {
        //Create a table only with board name, public and device has
        var finalTable = new DataTable("Boards_Features");
        int boardNameInx = dt.Columns.IndexOf("BoardName");
        int publicInx = dt.Columns.IndexOf("public");
        int deviceHasInx = dt.Columns.IndexOf("device_has");

        foreach (DataRow row in dt.Rows) {
            if (!String.IsNullOrEmpty(row[publicInx].ToString()) && row[publicInx].ToString().Equals("false")) {
                continue;
            }

            DataRow newRow = finalTable.NewRow();
            if (!finalTable.Columns.Contains("BoardName")) {
                finalTable.Columns.Add("BoardName");
            }
            newRow["BoardName"] = row[boardNameInx].ToString();

            if (!finalTable.Columns.Contains("device_has")) {
                finalTable.Columns.Add("device_has");
            }
            newRow["device_has"] = row[deviceHasInx].ToString();

            finalTable.Rows.Add(newRow);
        }

        AddAllContcatedColumns(finalTable, "device_has");

        try {
            finalTable.Columns.Remove("device_has");
        } catch (Exception ex) {
            _log.Error("FinlizeTable message" + ex.Message);
            throw ex;
        }
        _ds.Tables.Add(finalTable);
    }

    /// <summary>
    /// Get a concatanate column delimited by comma and create a cloumn for every entrance in the concatanate list
    /// </summary>
    /// <param name="dt">The table to work on</param>
    /// <param name="colName">The concatanate cloumn name to work on</param>
    private void AddAllContcatedColumns(DataTable dt, string colName)
    {
        int colInx = dt.Columns.IndexOf(colName);
        if (colInx == -1) {
            return;
        }

        foreach (DataRow row in dt.Rows) {
            var colData = row[colInx].ToString();
            if (String.IsNullOrEmpty(colData)) {
                continue;
            }

            var colDataList = colData.Split(',');
            foreach (var str in colDataList) {
                if (str.Equals("null")) {
                    continue;
                }

                if (!dt.Columns.Contains(str))
                    dt.Columns.Add(new DataColumn() {
                    ColumnName = str, DataType = System.Type.GetType("System.String"), DefaultValue = "false"
                });

                row[str] = "true";
            }
        }
    }

    /// <summary>
    /// Create a concatanate list of all the parents of the offspring.
    /// </summary>
    /// <param name="offspringRow">The data row of the offspring</param>
    /// <param name="dt">t=The working table. ususaly the normalize data table</param>
    /// <returns></returns>
    private IList<DataRow> GetAllParents(DataRow offspringRow, DataTable dt)
    {
        List<DataRow> parentsRows = new List<DataRow>();
        var inheritsCol = offspringRow.Field<string>(dt.Columns.IndexOf("inherits"));
        if (inheritsCol == null) {
            return parentsRows;
        }   

        var inherits = inheritsCol.Split(',');
        for (int i = inherits.Length - 1 ; i >= 0; i--) {
            string inherit = inherits[i];
            var parents = dt.Select("BoardName = '" + inherit.TrimEnd(' ').TrimStart(' ') + "'");

            if (parents.Length == 0) {         
               throw new Exception("missing parent in the table");
            }

            var tempParentsList = GetAllParents(parents[0], dt);
            if (tempParentsList != null) {
                parentsRows.AddRange(tempParentsList);
            }

            parentsRows.Add(parents[0]);
        }

        return parentsRows;
    }
}
}
