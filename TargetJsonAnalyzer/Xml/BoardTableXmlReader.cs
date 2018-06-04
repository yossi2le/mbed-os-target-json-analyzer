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
using TargetJsonAnalyzer.Interfaces.Data;
using log4net;

namespace TargetJsonAnalyzer.Xml {
/// <summary>
///  Boards table parser class.
/// </summary>
class BoardTableXmlReader : IBoardTableXmlReader {

    #region Private members
    /// <summary>
    /// Log4net logger object
    /// </summary>
    private ILog _log;

    /// <summary>
    /// Unity resolver singelthon.
    /// </summary>
    private IUnityContainer _unity;

    /// <summary>
    /// The boards data table.
    /// </summary>
    private DataTable _dt;

    /// <summary>
    /// Indicates if we start a new ellement
    /// </summary>
    private bool _start = false;

    /// <summary>
    /// Indicates if we satrt reading a new board
    /// </summary>
    private bool _newBoard = false;

    /// <summary>
    /// The name of the current workin board.
    /// We are assuming that the first board will always be target
    /// </summary>
    private String _currentBoard = "Target";

    /// <summary>
    /// Current working row
    /// </summary>
    private DataRow _dr = null;

    /// <summary>
    /// Config table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _configTableXmlReader = null;

    /// <summary>
    /// Overrides table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _overridesTableXmlReader = null;

    /// <summary>
    /// Post binary table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _postBinaryHookTableXmlReader = null;

    /// <summary>
    /// Expected soft device table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _expectedSoftdevicesWithoffsetsTableXmlReader = null;

    /// <summary>
    /// Progen table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _progenTableXmlReader = null;

    /// <summary>
    /// Target overrides table parsing objects
    /// </summary>
    private IExtendedDataInfoTable _targetOverridesTableXmlReader = null;

    /// <summary>
    /// Config table parsing objects
    /// </summary>
    private int _boardCount = 0;

    #endregion

    #region Public methods
    /// <summary>
    /// Constructor of the boards table
    /// </summary>
    public BoardTableXmlReader()
    {
        _log = LogManager.GetLogger("TargetJsonAnalyzer");
        _dt = new DataTable("Boards");
    }

    /// <summary>
    /// Load will call parse to start parsing all XML data. At the end all tables objects will be
    /// added to a collection of tables.
    /// </summary>
    /// <param name="board_xml">The XML buffer</param>
    /// <param name="unity">The unity resolver option</param>
    /// <exception cref="System.Exception">Throws exception on error</exception>
    public void Load(XmlReader board_xml, IUnityContainer unity)
    {
        try {
            _unity = unity;
            _start = false;
            _newBoard = false;
            _currentBoard = "Target";
            _dt.Columns.Add("Inx");
            _dt.Columns.Add("BoardName");
            _dr = null;
            while (board_xml.Read()) {
                ParseXml(board_xml);
            }

            Ds = new DataSet("Target Sqlite");
            Ds.Tables.Add(_dt);
            Ds.Tables.Add(_configTableXmlReader.Dt);
            Ds.Tables.Add(_postBinaryHookTableXmlReader.Dt);
            Ds.Tables.Add(_overridesTableXmlReader.Dt);
            Ds.Tables.Add(_expectedSoftdevicesWithoffsetsTableXmlReader.Dt);
            Ds.Tables.Add(_progenTableXmlReader.Dt);
            Ds.Tables.Add(_targetOverridesTableXmlReader.Dt);

            INormalizeData nd = _unity.Resolve<INormalizeData>();

            nd.Start(Ds);
        } catch (Exception ex) {
            _log.Error(String.Format("Failure. Please see message and log trace. message: {0} trace: {1}", ex.Message,
                                     ex.StackTrace));
            throw ex;
        }
    }

    /// <summary>
    /// Holds the collection of all tables
    /// </summary>
    public DataSet Ds { get; set; }
    #endregion

    #region Private methods
    /// <summary>
    /// ParseXml method for parsing the enrire XML file
    /// </summary>
    /// <param name="board_xml">the XML buffer</param>
    private void ParseXml(XmlReader board_xml)
    {
        if (board_xml.NodeType == XmlNodeType.Element && board_xml.Name == _currentBoard) {
            _start = true;
            _newBoard = true;
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "config")) {
            if (_configTableXmlReader == null) {
                _configTableXmlReader = _unity.Resolve<IConfigTableXmlReader>();
            }

            _configTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "post_binary_hook")) {
            if (_postBinaryHookTableXmlReader == null) {
                _postBinaryHookTableXmlReader = _unity.Resolve<IPostBinaryHookTable>();
            }

            _postBinaryHookTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "overrides")) {
            if (_overridesTableXmlReader == null) {
                _overridesTableXmlReader = _unity.Resolve<IOverridesTableXmlReader>();
            }

            _overridesTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "EXPECTED_SOFTDEVICES_WITH_OFFSETS")) {
            if (_expectedSoftdevicesWithoffsetsTableXmlReader == null) {
                _expectedSoftdevicesWithoffsetsTableXmlReader = _unity.Resolve<IExpectedSoftdevicesWithoffsetsTableXmlReader>();
            }

            _expectedSoftdevicesWithoffsetsTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "progen")) {
            if (_progenTableXmlReader == null) {
                _progenTableXmlReader = _unity.Resolve<IProgenTableXmlReader>();
            }

            _progenTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (board_xml.NodeType == XmlNodeType.Element && (board_xml.Name == "target_overrides")) {
            if (_targetOverridesTableXmlReader == null) {
                _targetOverridesTableXmlReader = _unity.Resolve<ITargetOverridesTableXmlReader>();
            }

            _targetOverridesTableXmlReader.Load(board_xml, _unity, _currentBoard);
        }

        if (!_start) {
            return;
        }

        if (board_xml.NodeType == XmlNodeType.Element && _newBoard == true) {
            _newBoard = false;
            _currentBoard = board_xml.Name;
            _dr = _dt.NewRow();
            _dr["Inx"] = ++_boardCount;
            _dr["BoardName"] = _currentBoard;
            _dt.Rows.Add(_dr);
        }

        if (board_xml.NodeType == XmlNodeType.Element && board_xml.Name != _currentBoard) {
            if (!_dt.Columns.Contains(board_xml.Name)) {
                _dt.Columns.Add(board_xml.Name);
            }
            var currentelementName = board_xml.Name;
            board_xml.Read();
            if (board_xml.NodeType == XmlNodeType.Text) {
                if (!_dt.Columns.Contains(currentelementName)) {
                    _dr[currentelementName] = board_xml.Value;
                } else {
                    var newValue = _dr[currentelementName].ToString() + "," + board_xml.Value;
                    newValue = newValue.TrimStart(new char[] { ',', ' ' });
                    _dr[currentelementName] = newValue;
                }
            } else if (board_xml.NodeType == XmlNodeType.Element) {
                _dr[currentelementName] = "null";
                ParseXml(board_xml);
            }
        }

        if (board_xml.NodeType == XmlNodeType.EndElement && board_xml.Name == _currentBoard) {
            _newBoard = true;
        }
    }
    #endregion

}
}
