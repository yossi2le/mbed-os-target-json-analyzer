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
using TargetJsonAnalyzer.Interfaces.Xml;
using Microsoft.Practices.Unity;
using System;
using System.Data;
using System.Xml;
using log4net;

namespace TargetJsonAnalyzer.Xml {
/// <summary>
/// Base calass for extended data tables.
/// </summary>
class BaseExtendedDataTableXmlReader : IExtendedDataInfoTable {

    #region Private members
    /// <summary>
    /// Unity resolver singelthon.
    /// </summary>
    private IUnityContainer _unity;

    /// <summary>
    /// Current board name
    /// </summary>
    private String _boardName = String.Empty;

    /// <summary>
    /// Current table name
    /// </summary>
    private string _tableName = String.Empty;
    #endregion

    #region Public methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tableName">Input: table name</param>
    /// <param name="elementName">Root element name for the parser</param>
    public BaseExtendedDataTableXmlReader(String tableName, String elementName)
    {
        Log = LogManager.GetLogger("TargetJsonAnalyzer");
        Dt = new DataTable(tableName);
        _tableName = elementName;
    }

    /// <summary>
    /// Entery point for the parser. this methos parse the XML
    /// </summary>
    /// <param name="board_xml">XML to parse</param>
    /// <param name="unity">Unity singleton for resolving objects</param>
    /// <param name="boardName">Current board we works on</param>
    virtual public void Load(XmlReader board_xml, IUnityContainer unity, String boardName)
    {
        _unity = unity;
        Dr = null;
        NewElement = true;
        _boardName = boardName;
        while (board_xml.Read()) {
            if (board_xml.NodeType == XmlNodeType.EndElement && board_xml.Name == _tableName) {
                break;
            }

            ParseXml(board_xml);
        }
    }

    /// <summary>
    /// The output table.
    /// </summary>
    public DataTable Dt { get; set; }
    #endregion

    #region Protected methods
    /// <summary>
    /// Current element name. this will cahnge as parsing is progeressing
    /// </summary>
    protected string CurrentElementName { get; set; }

    /// <summary>
    /// indicates if an ellement has been changed
    /// </summary>
    protected bool NewElement { get; set; }

    /// <summary>
    /// The current working row
    /// </summary>
    protected DataRow Dr { get; set; }

    /// <summary>
    /// The Logger option
    /// </summary>
    protected ILog Log { get; set; }

    /// <summary>
    /// Parse XML and create the expected table.
    /// </summary>
    /// <param name="board_xml">XML buffer to be parse</param>
    virtual protected void ParseXml(XmlReader board_xml)
    {
        if (board_xml.NodeType == XmlNodeType.EndElement && board_xml.Name == CurrentElementName) {
            NewElement = true;
        }

        if (board_xml.NodeType == XmlNodeType.Element && NewElement == true) {
            NewElement = false;
            CurrentElementName = board_xml.Name;
            Dr = Dt.NewRow();
            AddBoardFieldToTable();
            AddNameFieldToTable(CurrentElementName);
            Dt.Rows.Add(Dr);

            var currentelementName = board_xml.Name;
            board_xml.Read();
            if (board_xml.NodeType == XmlNodeType.Text) {
                AddValueFieldToTable(board_xml.Value);
            } else if (board_xml.NodeType == XmlNodeType.Element) {
                ParseXml(board_xml);
            }
        }
    }

    /// <summary>
    /// If table missing BoardName clumns it will be added here
    /// </summary>
    protected void AddBoardFieldToTable()
    {
        if (!Dt.Columns.Contains("BoardName")) {
            Dt.Columns.Add("BoardName");
        }
        Dr["BoardName"] = _boardName;
    }

    /// <summary>
    /// If table missing Name clumns it will be added here
    /// </summary>
    protected void AddNameFieldToTable(String name)
    {
        if (!Dt.Columns.Contains("Name")) {
            Dt.Columns.Add("Name");
        }
        Dr["Name"] = name;
    }

    /// <summary>
    /// If table missing Value clumns it will be added here
    /// </summary>
    protected void AddValueFieldToTable(String value)
    {
        if (!Dt.Columns.Contains("Value")) {
            Dt.Columns.Add("Value");
        }
        Dr["Value"] = value;
    }

    #endregion
}
}
