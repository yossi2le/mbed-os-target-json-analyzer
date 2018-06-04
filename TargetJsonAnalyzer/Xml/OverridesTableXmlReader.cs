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
/// This class repesent the overrides table
/// </summary>
class OverridesTableXmlReader : BaseExtendedDataTableXmlReader, IOverridesTableXmlReader {
    #region Private members
    /// <summary>
    /// This is the overrides table name.
    /// </summary>
    private static readonly string _tableName = "Overrides";
    /// <summary>
    /// This is the root element in the XML buffer for the overrides table.
    /// </summary>
    private static readonly string _rootElementName = "overrides";
    #endregion

    #region Public methods
    /// <summary>
    /// Constructor
    /// </summary>
    public OverridesTableXmlReader()
        : base( _tableName, _rootElementName) { }
    #endregion
}
}
