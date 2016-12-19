﻿// Copyright © 2008, 2010, Oracle and/or its affiliates. All rights reserved.
//
// MySQL for Visual Studio is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

namespace MySql.Data.VisualStudio.DbObjects
{
  internal class Metadata
  {
    /// <summary>
    /// Stores the list of allowed Data Types.
    /// </summary>
    private static List<string> DataTypes;

    public static bool IsStringType(string dataType)
    {
      dataType = dataType.ToLowerInvariant();
      int index = dataType.IndexOf('(');
      if (index != -1)
        dataType = dataType.Substring(0, index);

      return dataType.IndexOf("char") != -1 ||
             dataType.IndexOf("text") != -1;
    }

    public static string[] GetDataTypes(bool includeParens)
    {
      if (DataTypes == null)
        PopulateArray();
      string[] dataTypes = DataTypes.ToArray();
      if (!includeParens)
      {
        for (int i = 0; i < dataTypes.Length; i++)
          dataTypes[i] = RemoveParenthesis(dataTypes[i]);
      }
      return dataTypes;
    }

    /// <summary>
    /// Removes the parenthesis for the specified data type.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns></returns>
    private static string RemoveParenthesis(string dataType)
    {
      int index = dataType.IndexOf('(');
      if (index != -1)
        dataType = dataType.Substring(0, index);
      return dataType;
    }

    /// <summary>
    /// Populates the DataTypes array.
    /// </summary>
    private static void PopulateArray()
    {
      DataTypes = new List<string>();

      DataTypes.AddRange(new string[] {
            "bit()",
            "tinyint",
            "boolean",
            "smallint",
            "mediumint",
            "int",
            "serial",
            "float",
            "double",
            "decimal",
            "date",
            "datetime",
            "timestamp",
            "time",
            "year",
            "char()",
            "varchar()",
            "binary()",
            "varbinary()",
            "tinyblob",
            "tinytext",
            "blob",
            "text",
            "mediumblob",
            "mediumtext",
            "longblob",
            "longtext",
            "enum()",
            "set()"});
        }
  }
}
