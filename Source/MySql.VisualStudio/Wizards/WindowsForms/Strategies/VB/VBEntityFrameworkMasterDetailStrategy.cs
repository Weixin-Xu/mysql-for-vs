﻿// Copyright © 2008, 2014, Oracle and/or its affiliates. All rights reserved.
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySql.Data.VisualStudio.SchemaComparer;
using MySql.Data.VisualStudio.Wizards.WindowsForms;


namespace MySql.Data.VisualStudio.Wizards.WindowsForms
{
  internal class VBEntityFrameworkMasterDetailStrategy : VBMasterDetailStrategy
  {
    internal VBEntityFrameworkMasterDetailStrategy(StrategyConfig config)
      : base(config)
    { 
    }

    protected override void WriteUsingUserCode()
    {
      if (DataAccessTech == DataAccessTechnology.EntityFramework5)
      {
        Writer.WriteLine("Imports System.Data.Objects");
      }
      else if (DataAccessTech == DataAccessTechnology.EntityFramework6)
      {
        Writer.WriteLine("Imports System.Data.Entity.Core.Objects");
      }
    }

    protected override void WriteFormLoadCode()
    {
      Writer.WriteLine("ctx = New Model1Entities()");
      Writer.WriteLine("Dim _entities As ObjectResult(Of {0}) = ctx.{0}.Execute(MergeOption.AppendOnly)", CanonicalTableName);
      Writer.WriteLine("{0}BindingSource.DataSource = _entities", CanonicalTableName);

      foreach (KeyValuePair<string, Column> kvp in Columns)
      {
        string colName = kvp.Key;
        string idColumnCanonical = GetCanonicalIdentifier(colName);
        Writer.WriteLine("Me.{0}TextBox.DataBindings.Add(New System.Windows.Forms.Binding(\"Text\", Me.{2}BindingSource, \"{1}\", True ))",
          idColumnCanonical, colName, CanonicalTableName);
      }

      Writer.WriteLine("{0}BindingSource.DataSource = {1}BindingSource", CanonicalDetailTableName, CanonicalTableName);
      Writer.WriteLine("{0}BindingSource.DataMember = \"{0}\"", CanonicalDetailTableName );
      Writer.WriteLine("dataGridView1.DataSource = {0}BindingSource", CanonicalDetailTableName);
    }

    private void WriteValidationCodeIndividiualControl()
    {
      List<ColumnValidation> validationColumns = ValidationColumns;
      for (int i = 0; i < validationColumns.Count; i++)
      {
        ColumnValidation cv = validationColumns[i];
        string idColumnCanonical = GetCanonicalIdentifier(cv.Column.ColumnName);
        Writer.WriteLine("Private Sub {0}TextBox_Validating(sender As Object, e As CancelEventArgs)", idColumnCanonical);
        Writer.WriteLine("");
        Writer.WriteLine("  e.Cancel = False");
        if (cv.Required)
        {
          Writer.WriteLine("  If  String.IsNullOrEmpty( {0}TextBox.Text ) Then ", idColumnCanonical);
          Writer.WriteLine("    e.Cancel = True");
          Writer.WriteLine("    errorProvider1.SetError( {0}TextBox, \"The field {1} is required\" ) ", idColumnCanonical, cv.Name);
          Writer.WriteLine("  End If");
        }
        if (cv.IsNumericType())
        {
          Writer.WriteLine("  Dim v As Integer");
          Writer.WriteLine("  Dim s as String = {0}TextBox.Text", idColumnCanonical);
          Writer.WriteLine("  If Not Integer.TryParse( s, v ) Then");
          Writer.WriteLine("    e.Cancel = True");
          Writer.WriteLine("    errorProvider1.SetError( {0}TextBox, \"The field {1} must be numeric.\" )", idColumnCanonical, cv.Name);          
          if (cv.MinValue != null)
          {
            Writer.WriteLine(" ElseIf cv.MinValue > v Then ");
            Writer.WriteLine("   e.Cancel = True");
            Writer.WriteLine("   errorProvider1.SetError( {0}TextBox, \"The field {1} must be greater or equal than {2}.\" )", idColumnCanonical, cv.Name, cv.MinValue);
          }
          if (cv.MaxValue != null)
          {
            Writer.WriteLine(" ElseIf cv.MaxValue < v Then");
            Writer.WriteLine("   e.Cancel = True");
            Writer.WriteLine("   errorProvider1.SetError( {0}TextBox, \"The field {1} must be lesser or equal than {2}\" )", idColumnCanonical, cv.Name, cv.MaxValue);
          }
          Writer.WriteLine( " End If" );
        }
        Writer.WriteLine("  If Not e.Cancel Then" );
        Writer.WriteLine("    errorProvider1.SetError( {0}TextBox, \"\" )", idColumnCanonical);
        Writer.WriteLine("  End If");
        Writer.WriteLine("End Sub" );
        Writer.WriteLine();
      }
    }

    private void WriteValidationCodeDetailsGrid()
    {
      List<ColumnValidation> validationColumns = ValidationColumns;
      Writer.WriteLine("Private Sub dataGridView1_CellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs)");
      Writer.WriteLine("");

      Writer.WriteLine("  Dim v As Integer");
      Writer.WriteLine("  Dim s As String");
      Writer.WriteLine("  Dim row As DataGridViewRow = dataGridView1.Rows(e.RowIndex)");
      Writer.WriteLine("  Dim value As Object = e.FormattedValue");
      Writer.WriteLine("  e.Cancel = False");
      Writer.WriteLine("  row.ErrorText = \"\"");
      Writer.WriteLine("  If row.IsNewRow Then" );
      Writer.WriteLine("    Return");
      Writer.WriteLine("  End If" );
      for (int i = 0; i < validationColumns.Count; i++)
      {
        ColumnValidation cv = validationColumns[i];
        string idColumnCanonical = GetCanonicalIdentifier(cv.Column.ColumnName);

        Writer.WriteLine("  If e.ColumnIndex = {0} Then", i);
        Writer.WriteLine("  ");

        if (cv.Required)
        {
          Writer.WriteLine("  If  (TypeOf value is DBNull) OrElse String.IsNullOrEmpty( value.ToString() ) Then ");
          Writer.WriteLine("    e.Cancel = True");
          Writer.WriteLine("    row.ErrorText = \"The field {0} is required\"", cv.Name);
          Writer.WriteLine("    Return");
          Writer.WriteLine("  End If");
        }
        if (cv.IsNumericType())
        {
          Writer.WriteLine("  s = value.ToString()");
          Writer.WriteLine("  If Not Integer.TryParse( s, v ) Then");
          Writer.WriteLine("    e.Cancel = True");
          Writer.WriteLine("    row.ErrorText = \"The field {0} must be numeric.\"", cv.Name);
          Writer.WriteLine("    Return");
          if (cv.MinValue != null)
          {
            Writer.WriteLine(" ElseIf cv.MinValue > v Then");
            Writer.WriteLine("    e.Cancel = True");
            Writer.WriteLine("    row.ErrorText = \"The field {0} must be greater or equal than {1}.\"", cv.Name, cv.MinValue);
            Writer.WriteLine("    Return");
          }
          if (cv.MaxValue != null)
          {
            Writer.WriteLine(" ElseIf cv.MaxValue < v Then ");
            Writer.WriteLine("    e.Cancel = True");
            Writer.WriteLine("    row.ErrorText = \"The field {0} must be lesser or equal than {1}\"", cv.Name, cv.MaxValue);
            Writer.WriteLine("    Return");
          }
          Writer.WriteLine( "End If" );
        }

        Writer.WriteLine("  End If");
      }
      Writer.WriteLine("End Sub");
    }

    protected override void WriteValidationCode()
    {
      bool validationsEnabled = ValidationsEnabled;
      if (validationsEnabled)
      {
        WriteValidationCodeIndividiualControl();
        WriteValidationCodeDetailsGrid();
      }
    }

    protected override void WriteVariablesUserCode()
    {
      Writer.WriteLine("Private ctx As Model1Entities");
    }

    protected override void WriteSaveEventCode()
    {
      Writer.WriteLine("{0}BindingSource.EndEdit()", CanonicalTableName);
      Writer.WriteLine("{0}BindingSource.EndEdit()", CanonicalDetailTableName);
      Writer.WriteLine("ctx.SaveChanges()");
    }

    protected override void WriteDesignerControlDeclCode()
    {
      Writer.WriteLine("Friend WithEvents {0}BindingSource As System.Windows.Forms.BindingSource", CanonicalTableName);
      foreach (KeyValuePair<string, Column> kvp in Columns)
      {
        string idColumnCanonical = GetCanonicalIdentifier(kvp.Key);
        Writer.WriteLine("Friend WithEvents {0}TextBox As System.Windows.Forms.TextBox", idColumnCanonical);
        Writer.WriteLine("Friend WithEvents {0}Label As System.Windows.Forms.Label", idColumnCanonical);
      }
      Writer.WriteLine("Friend WithEvents {0}BindingSource As System.Windows.Forms.BindingSource", CanonicalDetailTableName);
      Writer.WriteLine("Friend WithEvents dataGridView1 As System.Windows.Forms.DataGridView");
    }

    protected override void WriteDesignerControlInitCode()
    {
      Writer.WriteLine("Me.bindingNavigator1.BindingSource = Me.{0}BindingSource", CanonicalTableName);
      WriteControlInitialization(false);
      // DataGrid
      Writer.WriteLine("Me.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize");
      Writer.WriteLine("Me.dataGridView1.Location = New System.Drawing.Point(280, 95)");
      Writer.WriteLine("Me.dataGridView1.Name = \"dataGridView1\" ");
      Writer.WriteLine("Me.dataGridView1.Size = New System.Drawing.Size(339, 261)");
      Writer.WriteLine("Me.dataGridView1.TabIndex = 0");
      if (ValidationsEnabled)
      {
        Writer.WriteLine("AddHandler Me.dataGridView1.CellValidating, AddressOf Me.dataGridView1_CellValidating");
      }
      Writer.WriteLine("Me.Controls.Add(Me.dataGridView1)");
    }

    protected override void WriteDesignerBeforeSuspendCode()
    {
      Writer.WriteLine("Me.dataGridView1 = New System.Windows.Forms.DataGridView()");
      Writer.WriteLine("Me.{0}BindingSource = New System.Windows.Forms.BindingSource(Me.components)", CanonicalTableName);
      Writer.WriteLine("Me.{0}BindingSource = New System.Windows.Forms.BindingSource(Me.components)", CanonicalDetailTableName);
    }

    protected override void WriteDesignerAfterSuspendCode()
    {
      Writer.WriteLine("CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()");
      Writer.WriteLine("CType(Me.{0}BindingSource, System.ComponentModel.ISupportInitialize).BeginInit()", CanonicalTableName);
      Writer.WriteLine("CType(Me.{0}BindingSource, System.ComponentModel.ISupportInitialize).BeginInit()", CanonicalDetailTableName);
    }

    protected override void WriteBeforeResumeSuspendCode()
    {
      Writer.WriteLine("Me.Size = New System.Drawing.Size(682, 590)");
      Writer.WriteLine("CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).EndInit()");
      Writer.WriteLine("CType(Me.{0}BindingSource, System.ComponentModel.ISupportInitialize).EndInit()", CanonicalDetailTableName);
      Writer.WriteLine("CType(Me.{0}BindingSource, System.ComponentModel.ISupportInitialize).EndInit()", CanonicalTableName);
    }
  }
}