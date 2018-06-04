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
using System.Xml;
using Microsoft.Practices.Unity;
using System;
using System.Data;

namespace TargetJsonAnalyzer.Interfaces.Xml {

/// <summary>
/// Interface for the exctended data info table, it is a base class.
/// </summary>
interface IExtendedDataInfoTable {

    /// <summary>
    /// Load function for parsing the xml into memory
    /// </summary>
    /// <param name="board_xml"></param>
    /// <param name="unity"></param>
    /// <param name="boardName"></param>
    void Load(XmlReader board_xml, IUnityContainer unity, String boardName);

    /// <summary>
    /// Set and Get the inner data table.
    /// </summary>
    DataTable Dt { get; set; }
}
}