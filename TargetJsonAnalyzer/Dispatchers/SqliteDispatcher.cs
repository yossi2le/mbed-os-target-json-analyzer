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
using System.Data.SQLite;
using System.IO;
using TargetJsonAnalyzer.Config;
using TargetJsonAnalyzer.Interfaces.Dispatchers;

namespace TargetJsonAnalyzer.Dispatchers {

/// <summary>
/// Excel output dispatcher class
/// </summary>
class SqliteDispatcher : ISqliteDispatcher {

    /// <summary>
    ///
    /// </summary>
    private DataSet _ds = null;

    /// <summary>
    ///
    /// </summary>
    private ILog _log = LogManager.GetLogger("TargetJsonAnalyzer");

    /// <summary>
    ///
    /// </summary>
    private Boolean _isInitializa = false;

    /// <summary>
    ///
    /// </summary>
    /// <param name="ds"></param>
    public void Init( DataSet ds)
    {
        _ds = ds;
        _isInitializa = true;
    }

    /// <summary>
    ///
    /// </summary>
    public void Dispatch()
    {
        if (!_isInitializa) {
            throw new Exception("Excel Dispatcher is not initialize");
        }

        if (!ImportDataToSQLiteDatabase(_ds)) {
            throw new Exception("Sqlite Dispatcher has finished with error");
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ds"></param>
    /// <returns></returns>
    public bool ImportDataToSQLiteDatabase(DataSet ds)
    {
        try {
            var SQLiteDatabase = Configuration.Instance.OutputFolderName + "\\" + Path.GetFileNameWithoutExtension(
                                     Configuration.Instance.InputFileName) + ".db";

            foreach (DataTable result in ds.Tables) {
                string insertCmd = "insert into " + result.TableName + "(";
                string createCmd = "create table " + result.TableName + "(";
                string colCollection = String.Empty;
                string createCollection = String.Empty;
                foreach (DataColumn col in result.Columns) {
                    colCollection += ",'" + col.ColumnName + "'";
                    colCollection = colCollection.TrimStart(',');
                    createCollection += ",'" + col.ColumnName + "' TEXT";
                    createCollection = createCollection.TrimStart(',');
                }
                insertCmd += colCollection + ") values (";
                createCmd += createCollection + ")";

                using (SQLiteConnection con = new SQLiteConnection(
                    string.Format("Data Source={0};Version=3;New=False;Compress=True;Max Pool Size=100;", SQLiteDatabase))) {
                    con.Open();
                    using (SQLiteCommand sqlitecommand = new SQLiteCommand(createCmd, con)) {
                        sqlitecommand.ExecuteNonQuery();
                    }

                    using (SQLiteTransaction transaction = con.BeginTransaction()) {
                        foreach (DataRow row in result.Rows) {
                            string specificInsertCmd = insertCmd;
                            for (int i = 0; i < result.Columns.Count; i++) {
                                string value = row[i].ToString();
                                specificInsertCmd += "'" + value + "',";
                            }
                            specificInsertCmd = specificInsertCmd.TrimEnd(',');
                            specificInsertCmd += ")";

                            using (SQLiteCommand sqlitecommand = new SQLiteCommand(specificInsertCmd, con)) {
                                sqlitecommand.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        _log.Info("Data successfully imported for table " + result.TableName);
                    }
                }
            }
            return true;
        } catch (Exception ex) {
            _log.Error("Import data to sqlite has failed. message:" + ex.Message);
            return false;
        }
        finally {

        }
    }
}
}
