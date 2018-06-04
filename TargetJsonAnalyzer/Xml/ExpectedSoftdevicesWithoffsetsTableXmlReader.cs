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

namespace TargetJsonAnalyzer.Xml {
/// <summary>
/// This class repesent the Expected softdevices with offset table
/// </summary>
class ExpectedSoftdevicesWithoffsetsTableXmlReader : BaseExtendedDataTableXmlReader,
    IExpectedSoftdevicesWithoffsetsTableXmlReader {

    #region Private members
    /// <summary>
    /// This is the Config table name.
    /// </summary>
    private static readonly string _tableName = "ExpectedSoftdevicesWithoffsets";

    /// <summary>
    /// This is the root element in the XML buffer.
    /// </summary>
    private static readonly string _rootElementName = "EXPECTED_SOFTDEVICES_WITH_OFFSETS";
    #endregion

    #region Public methods
    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectedSoftdevicesWithoffsetsTableXmlReader()
        : base(_tableName, _rootElementName) { }
    #endregion
}
}
