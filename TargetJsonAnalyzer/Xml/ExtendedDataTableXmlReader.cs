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
using System;
using System.Xml;

namespace TargetJsonAnalyzer.Xml {
/// <summary>
/// Extended data table class. This is a parser class on top of BaseExtendedDataTableXmlReader
/// to help parsing the XML data into tables.
/// </summary>
class ExtendedDataTableXmlReader : BaseExtendedDataTableXmlReader, IExtendedDataInfoTable {

    #region Public methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tableName">The table name this object represent</param>
    /// <param name="elementName">The root element name</param>
    public ExtendedDataTableXmlReader(String tableName, String elementName)
        : base (tableName, elementName) { }
    #endregion

    #region Protected methods
    /// <summary>
    /// Parse XML function for extended data tables
    /// </summary>
    /// <param name="board_xml"></param>
    override protected void ParseXml(XmlReader board_xml)
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
        }

        if (board_xml.NodeType == XmlNodeType.Element && board_xml.Name != CurrentElementName) {
            if (!Dt.Columns.Contains(board_xml.Name)) {
                Dt.Columns.Add(board_xml.Name);
            }

            var currentelementName = board_xml.Name;
            board_xml.Read();
            if (board_xml.NodeType == XmlNodeType.Text) {
                Dr[currentelementName] = board_xml.Value;
            } else if (board_xml.NodeType == XmlNodeType.Element) {
                ParseXml(board_xml);
            }
        }
    }
    #endregion
}
}
