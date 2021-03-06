﻿// Copyright © 2015, 2016 Oracle and/or its affiliates. All rights reserved.
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

using EnvDTE;
using MySql.Data.MySqlClient;
using MySql.Data.VisualStudio.SchemaComparer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using MySql.Data.VisualStudio.Properties;
using MySql.Utility.Classes.MySql;
using VSLangProj;

namespace MySql.Data.VisualStudio.Wizards.ItemTemplates
{
  /// <summary>
  /// Item Templates Utilities
  /// </summary>
  public class ItemTemplateUtilities
  {
    private static string[] resourcesFiles = new string[] { "bindingNavigatorAddNewItem.Image.png", "bindingNavigatorDeleteItem.Image.png", "bindingNavigatorMoveFirstItem.Image.png"
                                                    , "bindingNavigatorMoveLastItem.Image.png", "bindingNavigatorMoveNextItem.Image.png", "bindingNavigatorMovePreviousItem.Image.png"
                                                    , "bindingNavigatorSaveItem.Image.png" };

    /// <summary>
    /// Gets the provider connection string from an "edmx" file.
    /// </summary>
    /// <param name="edmxFileName">Name of the edmx file.</param>
    /// <param name="dte">The DTE object.</param>
    /// <param name="checkForAppConfig">if set to <c>true</c>, gets the provider conn. string from the app.config file.
    /// Otherwise, will get the provider conn. string from the web.config file.</param>
    /// <returns>The provider connection string.</returns>
    internal static string GetProviderConnectionString(string edmxFileName, DTE dte, bool checkForAppConfig)
    {
      if (string.IsNullOrEmpty(edmxFileName))
      {
        return string.Empty;
      }

      string connString = string.Empty;
      edmxFileName = edmxFileName.Contains(".edmx") ? edmxFileName.Replace(".edmx", "") : edmxFileName;
      XElement webConfig = GetWebConfig(edmxFileName, dte, checkForAppConfig);
      if (webConfig != null)
      {
        XElement connStringsSection = webConfig.Element("connectionStrings");
        if (connStringsSection != null)
        {
          XElement connectrionString = connStringsSection.Elements().FirstOrDefault(a => a.Attribute("connectionString").Value.Contains("MySqlClient")
                                        && a.Attribute("connectionString").Value.Contains(edmxFileName));
          connString = new EntityConnectionStringBuilder(connectrionString.Attribute("connectionString").Value).ProviderConnectionString;
          return connString;
        }
      }

      return connString;
    }

    /// <summary>
    /// Gets the connection string from an "edmx" file.
    /// </summary>
    /// <param name="edmxFileName">Name of the edmx file.</param>
    /// <param name="dte">The DTE object.</param>
    /// <param name="checkForAppConfig">if set to <c>true</c>, gets the conn. string from the app.config file.
    /// Otherwise, will get the conn. string from the web.config file.</param>
    /// <returns>The connection string.</returns>
    internal static string GetConnectionStringName(string edmxFileName, DTE dte, bool checkForAppConfig)
    {
      if (string.IsNullOrEmpty(edmxFileName))
      {
        return string.Empty;
      }

      string connStringName = string.Empty;
      edmxFileName = edmxFileName.Contains(".edmx") ? edmxFileName.Replace(".edmx", "") : edmxFileName;
      XElement webConfig = GetWebConfig(edmxFileName, dte, checkForAppConfig);
      if (webConfig != null)
      {
        XElement connStringsSection = webConfig.Element("connectionStrings");
        if (connStringsSection != null)
        {
          XElement connectrionString = connStringsSection.Elements().FirstOrDefault(a => a.Attribute("connectionString").Value.Contains("MySqlClient")
                                        && a.Attribute("connectionString").Value.Contains(edmxFileName));
          connStringName = connectrionString.Attribute("name").Value;
          return connStringName;
        }
      }

      return connStringName;
    }

    /// <summary>
    /// Gets the web config XML from the "edmx" file.
    /// </summary>
    /// <param name="edmxFileName">Name of the edmx file.</param>
    /// <param name="dte">The DTE object.</param>
    /// <param name="checkForAppConfig">if set to <c>true</c>, gets the conn. string from the app.config file.
    /// Otherwise, will get the conn. string from the web.config file.</param>
    /// <returns>The XElement XML of the web config.</returns>
    internal static XElement GetWebConfig(string edmxFileName, DTE dte, bool checkForAppConfig)
    {
      if (string.IsNullOrEmpty(edmxFileName))
      {
        return null;
      }

      string projectPath = string.Empty;
      Array activeProjects = (Array)dte.ActiveSolutionProjects;
      if (activeProjects.Length > 0)
      {
        Project activeProj = (Project)activeProjects.GetValue(0);
        VSProject vsProj = activeProj.Object as VSProject;
        projectPath = System.IO.Path.GetDirectoryName(activeProj.FullName);
      }

      XElement webConfig = null;
      if (!string.IsNullOrEmpty(projectPath))
      {
        try
        {
          webConfig = XElement.Load(string.Format(@"{0}\{1}", projectPath, checkForAppConfig ? "app.config" : "web.config"));
        }
        catch (Exception ex)
        {
          MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_ConfigFileGetError, true);
        }
      }

      return webConfig;
    }

    /// <summary>
    /// Gets the web config XML from the project physical path.
    /// </summary>
    /// <param name="projectPath">The path of the project.</param>
    /// <param name="checkForAppConfig">if set to <c>true</c>, gets the conn. string from the app.config file.
    /// Otherwise, will get the conn. string from the web.config file</param>
    /// <returns>The XElement XML of the web config.</returns>
    internal static XElement GetWebConfig(string projectPath, bool checkForAppConfig)
    {
      if (string.IsNullOrEmpty(projectPath))
      {
        return null;
      }

      XElement webConfig = null;
      try
      {
        webConfig = XElement.Load(string.Format(@"{0}\{1}", projectPath, checkForAppConfig ? "app.config" : "web.config"));
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_ConfigFileGetError, true);
      }

      // Try to get the config XML from the app.config file.
      if (webConfig == null && !checkForAppConfig)
      {
        GetWebConfig(projectPath, true);
      }

      return webConfig;
    }

    /// <summary>
    /// Gets all the existing "edmx" models that exists in the project.
    /// </summary>
    /// <param name="projectItems">The project items.</param>
    /// <param name="models">The models list to be filled.</param>
    /// <returns>A list of string with all the "edmx" that exists in the project.</returns>
    internal static List<string> GetModels(ProjectItems projectItems, ref List<string> models)
    {
      foreach (ProjectItem item in projectItems)
      {
        if (item.get_FileNames(1).Contains(".edmx") && !item.get_FileNames(1).Contains(".edmx.diagram"))
        {
          models.Add(item.Name.Replace(".edmx", ""));
        }

        if (item.SubProject != null)
        {
          // We navigate recursively because it can be an Enterprise project or a solution folder
          GetModels(item.SubProject.ProjectItems, ref models);
        }
        else if (item.ProjectItems.Count > 0)
        {
          // We navigate recursively because it can be a folder inside a project or a project item with nested project items (code-behind files, etc.)
          GetModels(item.ProjectItems, ref models);
        }
      }

      return models;
    }

    /// <summary>
    /// From an "edmx" model, gets the Entity Framework version used.
    /// </summary>
    /// <param name="_dte">The DTE object.</param>
    /// <param name="selectedModel">The selected "edmx" model.</param>
    /// <param name="checkForAppConfig">if set to <c>true</c>, gets the conn. string from the app.config file.
    /// Otherwise, will get the conn. string from the web.config file</param>
    /// <returns></returns>
    internal static DataAccessTechnology GetEntityFrameworkVersion(DTE _dte, string selectedModel, bool checkForAppConfig)
    {
      if (_dte == null || string.IsNullOrEmpty(selectedModel))
      {
        return DataAccessTechnology.None;
      }

      Array activeProjects = (Array)_dte.ActiveSolutionProjects;
      if (activeProjects.Length > 0)
      {
        Project activeProj = (Project)activeProjects.GetValue(0);
        VSProject vsProj = activeProj.Object as VSProject;
        string projectPath = System.IO.Path.GetDirectoryName(activeProj.FullName);
        XElement configFile = GetWebConfig(projectPath, checkForAppConfig);
        if (configFile != null)
        {
          XElement entityFrameworkSection = configFile.Elements("configSections").Elements("section")
                                              .FirstOrDefault(a => a.Attribute("name") != null && a.Attribute("name").Value == "entityFramework");
          if (entityFrameworkSection != null)
          {
            return entityFrameworkSection.Attribute("type").Value.Contains("Version=5")
              ? DataAccessTechnology.EntityFramework5
              : entityFrameworkSection.Attribute("type").Value.Contains("Version=6")
                ? DataAccessTechnology.EntityFramework6
                : DataAccessTechnology.None;
          }
        }
      }

      return DataAccessTechnology.None;
    }

    /// <summary>
    /// Retrieves all the information for a given Foreign Key, for the specified table.
    /// </summary>
    /// <param name="con">The MySql connection.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="MyFKs">A dictionary containing the information for the related foreign keys.</param>
    internal static void RetrieveAllFkInfo(MySqlConnection con, string tableName, out Dictionary<string, ForeignKeyColumnInfo> MyFKs)
    {
      string sql = string.Format(@"select `constraint_name`, `table_name`, `column_name`, `referenced_table_name`, `referenced_column_name`
                                  from information_schema.key_column_usage where table_schema = '{0}' and `constraint_name` in
                                  (select `constraint_name` from information_schema.referential_constraints where `constraint_schema` = '{0}' and `table_name` = '{1}')", con.Database, tableName);
      if ((con.State & ConnectionState.Open) == 0)
      {
        con.Open();
      }

      Dictionary<string, ForeignKeyColumnInfo> FKs = new Dictionary<string, ForeignKeyColumnInfo>();
      // Gather FK info per column pair
      try
      {
        MySqlCommand cmd = new MySqlCommand(sql, con);
        using (MySqlDataReader r = cmd.ExecuteReader())
        {
          while (r.Read())
          {
            ForeignKeyColumnInfo fk = new ForeignKeyColumnInfo()
            {
              ConstraintName = r.GetString(0),
              TableName = r.GetString(1),
              ColumnName = r.GetString(2),
              ReferencedTableName = r.GetString(3),
              ReferencedColumnName = r.GetString(4)
            };

            FKs.Add(fk.ColumnName, fk);
          }
        }
      }
      catch (MySqlException ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_RetrieveFkError, true);
      }

      // Gather referenceable columns
      foreach (ForeignKeyColumnInfo fk in FKs.Values)
      {
        fk.ReferenceableColumns = GetColumnsFromTableVanilla(fk.ReferencedTableName, con);
      }

      MyFKs = FKs;
    }

    /// <summary>
    /// Gets the columns from table vanilla.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="con">The MySql connection.</param>
    /// <returns>A list of string with all the columns from table Vanilla.</returns>
    private static List<string> GetColumnsFromTableVanilla(string tableName, MySqlConnection con)
    {
      string sql = string.Format(@"select c.column_name from information_schema.columns c  where ( c.table_schema = '{0}' ) and ( c.table_name = '{1}' );", con.Database, tableName);
      List<string> columns = new List<string>();
      try
      {
        MySqlCommand cmd = new MySqlCommand(sql, con);
        using (MySqlDataReader r = cmd.ExecuteReader())
        {
          while (r.Read())
          {
            columns.Add(r.GetString(0));
          }
        }
      }
      catch (MySqlException ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_RetrieveColumnsError, true);
      }

      return columns;
    }

    /// <summary>
    /// Gets the canonical identifier.
    /// </summary>
    /// <param name="Identifier">The identifier.</param>
    /// <returns>The canonical identifier.</returns>
    internal static string GetCanonicalIdentifier(string Identifier)
    {
      if (Identifier == null)
      {
        return "";
      }

      char[] chars = Identifier.ToCharArray();
      for (int i = 0; i < chars.Length; i++)
      {
        if (!char.IsLetterOrDigit(chars[i]))
        {
          chars[i] = '_';
        }
      }

      return new string(chars);
    }

    /// <summary>
    /// Gets the columns from a specific table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="con">The MySql connection.</param>
    /// <returns></returns>
    internal static Dictionary<string, Column> GetColumnsFromTable(string tableName, MySqlConnection con)
    {
      string sqlFilter = string.Format(@"select t.table_name from information_schema.tables t
                                        where ( t.table_schema = '{0}' ) and ( t.table_name = '{1}' )", con.Database, tableName);
      string sqlData = string.Format(@"select c.table_schema, c.table_name, c.column_name, c.column_default, c.is_nullable, c.data_type,
                                        c.character_maximum_length, c.numeric_precision, c.numeric_scale,  c.column_type
                                        from information_schema.columns c where ( c.table_schema = '{0}' ) and ( c.table_name in {1} )", con.Database, "{0}");
      Dictionary<string, Column> dic = GetMetadata<Column>(con, sqlFilter, sqlData);
      return dic;
    }

    /// <summary>
    /// Gets the metadata information for a specific sql command query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="sqlFilter">The SQL filter command.</param>
    /// <param name="sqlData">The SQL data.</param>
    /// <returns>A dictionary with the metadata information.</returns>
    private static Dictionary<string, T> GetMetadata<T>(MySqlConnection connection, string sqlFilter, string sqlData) where T : MetaObject, new()
    {
      Dictionary<string, T> dic = new Dictionary<string, T>();
      if (connection != null && (connection.State & ConnectionState.Open) == 0)
      {
        connection.Open();
      }

      try
      {
        MySqlCommand cmd = new MySqlCommand(string.Empty, connection);
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(sqlFilter))
        {
          sb.Append("( ");
          cmd.CommandText = sqlFilter;

          using (MySqlDataReader r1 = cmd.ExecuteReader())
          {
            while (r1.Read())
            {
              sb.Append("'").Append(r1.GetString(0)).Append("',");
            }
          }

          sb.Length = sb.Length - 1;
          sb.Append(" ) ");
          cmd.CommandText = string.Format(sqlData, sb);
        }
        else
        {
          cmd.CommandText = sqlData;
        }

        // Get columns
        using (MySqlDataReader r = cmd.ExecuteReader())
        {
          while (r.Read())
          {
            T t = new T();
            t.Connection = connection;
            t.Initialize(r);
            dic.Add(t.Name, t);
          }
        }
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_RetrieveMetadataError, true);
      }

      return dic;
    }

    /// <summary>
    /// Generates the entity framework model.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="vsProj">The Visual Studio project object.</param>
    /// <param name="connection">The MySql connection.</param>
    /// <param name="modelName">Name of the "edmx" model.</param>
    /// <param name="tables">The tables.</param>
    /// <param name="modelPath">The model path.</param>
    /// <param name="currentEntityFrameworkVersion">The current entity framework version.</param>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    /// <param name="columnMappings">The column mappings.</param>
    /// <param name="tablesIncludedInModel">The tables included in model.</param>
    internal static void GenerateEntityFrameworkModel(Project project, VSProject vsProj, MySqlConnection connection, string modelName, List<string> tables, string modelPath,
      string currentEntityFrameworkVersion, LanguageGenerator language, Dictionary<string, Dictionary<string, ColumnValidation>> columnMappings, ref Dictionary<string, string> tablesIncludedInModel)
    {
      if (project != null)
      {
        string projectNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();
        string ns = GetCanonicalIdentifier(projectNamespace);
        EntityFrameworkGenerator gen = new EntityFrameworkGenerator(connection, modelName, tables, modelPath, ns, currentEntityFrameworkVersion, language, vsProj, columnMappings);
        vsProj = project.Object as VSProject;
        project.DTE.Solution.SolutionBuild.Build(true);
        string projectPath = Path.GetDirectoryName(project.FullName);
        gen.GenerateItemTemplates(projectPath, modelName);
        if (gen.TablesInModel.Count() > 0)
        {
          tablesIncludedInModel = gen.TablesInModel.ToDictionary<string, string>(p => p);
        }

        TryErrorsEntityFrameworkGenerator(gen);
      }
    }

    /// <summary>
    /// Validates whether the Entity Framework generator has any errors.
    /// </summary>
    /// <param name="gen">The EntityFrameworkGenerator generator.</param>
    /// <exception cref="WizardException"></exception>
    private static void TryErrorsEntityFrameworkGenerator(EntityFrameworkGenerator gen)
    {
      if (gen == null || gen.Errors.Count() == 0)
      {
        return;
      }

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < gen.Errors.Count(); i++)
      {
        sb.Append(" - ").AppendLine(gen.Errors.ElementAt(i));
      }

      throw new WizardException(string.Format("The Entity Framework generation failed with the following errors:\n\n", sb.ToString()));
    }

    /// <summary>
    /// Ensures the code is generated for a date time grid column.
    /// </summary>
    /// <param name="vsProj">The vs proj.</param>
    /// <param name="columns">The columns.</param>
    /// <param name="detailColumns">The detail columns.</param>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    /// <param name="projectPath">The project path.</param>
    /// <param name="projectNamespace">The project namespace.</param>
    /// <returns></returns>
    internal static bool EnsureCodeForDateTimeGridColumn(VSProject vsProj, Dictionary<string, Column> columns, Dictionary<string, Column> detailColumns, LanguageGenerator language,
      string projectPath, string projectNamespace)
    {
      bool hasDateColumn = false;
      if (columns != null)
      {
        foreach (KeyValuePair<string, Column> kvp in columns)
        {
          if (kvp.Value.IsDateType())
          {
            hasDateColumn = true;
            break;
          }
        }
      }

      if (!hasDateColumn && detailColumns != null)
      {
        foreach (KeyValuePair<string, Column> kvp in detailColumns)
        {
          if (kvp.Value.IsDateType())
          {
            hasDateColumn = true;
            break;
          }
        }
      }

      // If is the case, then add support code.
      if (hasDateColumn)
      {
        string outFilePath = "";
        Stream stream = null;
        switch (language)
        {
          case LanguageGenerator.CSharp:
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MySql.Data.VisualStudio.Wizards.WindowsForms.Templates.CS.MyDateTimePickerColumn.cs");
            outFilePath = Path.Combine(projectPath, "MyDateTimePickerColumn.cs");
            break;
          case LanguageGenerator.VBNET:
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MySql.Data.VisualStudio.Wizards.WindowsForms.Templates.VB.MyDateTimePickerColumn.vb");
            outFilePath = Path.Combine(projectPath, "MyDateTimePickerColumn.vb");
            break;
        }

        if (stream != null)
        {
          using (var sr = new StreamReader(stream))
          {
            string contents = sr.ReadToEnd();
            File.WriteAllText(outFilePath, contents.Replace("$ProjectNamespace$", projectNamespace));
          }
        }

        vsProj.Project.ProjectItems.AddFromFile(outFilePath);
      }

      return hasDateColumn;
    }

    /// <summary>
    /// Gets the Connection string with included password.
    /// </summary>
    /// <param name="connection">The MySql connection.</param>
    /// <returns>The MySql connection string, or empty if the Connection Settings could not be retrieved.</returns>
    internal static string ConnectionStringWithIncludedPassword(MySqlConnection connection)
    {
      MySqlConnectionStringBuilder msb = GetConnectionSettings(connection);
      return msb != null ? msb.ToString() : string.Empty;
    }

    /// <summary>
    /// Gets the connection settings.
    /// </summary>
    /// <param name="connection">The MySql connection.</param>
    /// <returns>The connection settings for a connection string, or null if the settings could not be retrieved.</returns>
    private static MySqlConnectionStringBuilder GetConnectionSettings(MySqlConnection connection)
    {
      if (connection == null)
      {
        return null;
      }

      try
      {
        Type t = typeof(MySqlConnection);
        PropertyInfo p = t.GetProperty("Settings", BindingFlags.NonPublic | BindingFlags.Instance);
        return (MySqlConnectionStringBuilder)p.GetValue(connection, null);
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, null, Resources.ItemTemplateUtilities_RetrieveSettingsError, true);
        return null;
      }
    }

    /// <summary>
    /// Adds the new form to the current project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="formName">Name of the form.</param>
    /// <param name="projectPath">The project path.</param>
    /// <param name="projectNamespace">The project namespace.</param>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    internal static void AddNewForm(Project project, string formName, string projectPath, string projectNamespace, LanguageGenerator language)
    {
      if (string.IsNullOrEmpty(projectPath))
      {
        projectPath = Path.GetDirectoryName(project.FullName);
      }

      if (string.IsNullOrEmpty(projectNamespace))
      {
        projectNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();
      }

      string formFile = Path.Combine(projectPath, language == LanguageGenerator.CSharp
                                                    ? "FormMySQL1.cs".Replace("FormMySQL1", formName)
                                                    : "FormMySQL1.vb".Replace("FormMySQL1", formName));
      string formFileDesigner = Path.Combine(projectPath, language == LanguageGenerator.CSharp
                                                    ? "FormMySQL1.Designer.cs".Replace("FormMySQL1", formName)
                                                    : "FormMySQL1.Designer.vb".Replace("FormMySQL1", formName));
      string formFileResx = Path.Combine(projectPath, "FormMySQL1.resx".Replace("FormMySQL1", formName));
      string contents = "";
      contents = File.ReadAllText(Path.Combine(projectPath, language == LanguageGenerator.CSharp ? "FormMySQL1.cs" : "FormMySQL1.vb"));
      contents = contents.Replace("FormMySQL1", formName);
      contents = contents.Replace("$safeprojectname$", projectNamespace);
      File.WriteAllText(formFile, contents);
      contents = File.ReadAllText(Path.Combine(projectPath, language == LanguageGenerator.CSharp ? "FormMySQL1.Designer.cs" : "FormMySQL1.Designer.vb"));
      contents = contents.Replace("FormMySQL1", formName);
      contents = contents.Replace("$safeprojectname$", projectNamespace);
      File.WriteAllText(formFileDesigner, contents);
      contents = File.ReadAllText(Path.Combine(projectPath, "FormMySQL1.resx"));
      contents = contents.Replace("FormMySQL1", formName);
      contents = contents.Replace("$safeprojectname$", projectNamespace);
      File.WriteAllText(formFileResx, contents);
      // Now add the form
      ProjectItem pi = project.ProjectItems.AddFromFile(formFile);
      ProjectItem pi2 = pi.ProjectItems.AddFromFile(formFileDesigner);
      ProjectItem pi3 = pi.ProjectItems.AddFromFile(formFileResx);
      pi3.Properties.Item("ItemType").Value = "EmbeddedResource";
      pi.Properties.Item("SubType").Value = "Form";
    }

    /// <summary>
    /// Finds an specific item in the peoject.
    /// </summary>
    /// <param name="items">The project items.</param>
    /// <param name="name">The name of the item to find.</param>
    /// <returns></returns>
    internal static ProjectItem FindProjectItem(ProjectItems items, string name)
    {
      ProjectItem item = null;
      for (int i = 1; i <= items.Count; i++)
      {
        ProjectItem item2 = items.Item(i);
        if (item2.Name != name)
        {
          continue;
        }

        item = item2;
        break;
      }

      return item;
    }

    /// <summary>
    /// Removes the temporary template form used to create the item template.
    /// </summary>
    /// <param name="proj">The VS project.</param>
    /// <param name="projectPath">The project path.</param>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    internal static void RemoveTemplateForm(VSProject proj, string projectPath, LanguageGenerator language)
    {
      if (proj != null && !string.IsNullOrEmpty(projectPath))
      {
        string formName = language == LanguageGenerator.CSharp ? "FormMySQL1.cs" : "FormMySQL1.vb";
        string formDesigner = language == LanguageGenerator.CSharp ? "FormMySQL1.Designer.cs" : "FormMySQL1.Designer.vb";
        ProjectItem pi = proj.Project.ProjectItems.Item(formName);
        pi.Remove();
        File.Delete(Path.Combine(projectPath, formName));
        File.Delete(Path.Combine(projectPath, formDesigner));
        File.Delete(Path.Combine(projectPath, "FormMySQL1.resx"));
      }
    }

    /// <summary>
    /// Fixes the namespaces for the created Visual Basic.NET item template.
    /// </summary>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    /// <param name="projectPath">The project path.</param>
    /// <param name="projectNamespace">The project namespace.</param>
    /// <param name="dataAccessTechnology">The data access technology (EF5 or EF6).</param>
    internal static void FixNamespaces(LanguageGenerator language, string projectPath, string projectNamespace, DataAccessTechnology dataAccessTechnology)
    {
      if (language != LanguageGenerator.VBNET)
      {
        return;
      }

      string outputPath = Path.Combine(Path.Combine(projectPath, "My Project"), "Application.Designer.vb");
      string contents = File.ReadAllText(outputPath);
      if (dataAccessTechnology == DataAccessTechnology.EntityFramework6)
      {
        contents = contents.Replace(string.Format("Me.MainForm = Global.{0}.frmMain", projectNamespace),
                                    string.Format("Me.MainForm = Global.{0}.{0}.frmMain", projectNamespace));
      }
      else
      {
        contents = contents.Replace(string.Format("Me.MainForm = Global.{0}.frmMain", projectNamespace),
                                    string.Format("Me.MainForm = {0}.frmMain", projectNamespace));
      }

      File.WriteAllText(outputPath, contents);
    }

    /// <summary>
    /// Updates the name of the model.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="formName">Name of the form.</param>
    /// <param name="projectPath">The project path.</param>
    /// <param name="projectNamespace">The project namespace.</param>
    /// <param name="newModelName">New name of the model.</param>
    /// <param name="language">The language generator (C# or VB.NET).</param>
    internal static void UpdateModelName(Project project, string formName, string projectPath, string projectNamespace, string newModelName, LanguageGenerator language)
    {
      if (string.IsNullOrEmpty(projectPath))
      {
        projectPath = Path.GetDirectoryName(project.FullName);
      }

      if (string.IsNullOrEmpty(projectNamespace))
      {
        projectNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();
      }

      string formFile = Path.Combine(projectPath, language == LanguageGenerator.CSharp ? string.Format("{0}.cs", formName) : string.Format("{0}.vb", formName));
      string contents = "";
      contents = File.ReadAllText(Path.Combine(projectPath, formFile));
      contents = contents.Replace("Model1Entities", string.Format("{0}", newModelName));
      File.WriteAllText(formFile, contents);
    }

    /// <summary>
    /// Copies the resources used by the windows forms templates to the project.
    /// </summary>
    /// <param name="vsProj">The vs proj.</param>
    /// <param name="itemTemplateTempPath">The item template temporary path.</param>
    internal static void CopyResourcesToProject(Project vsProj, string itemTemplateTempPath)
    {
      if (vsProj == null || string.IsNullOrEmpty(itemTemplateTempPath))
      {
        return;
      }

      ProjectItem resourcesFolder;
      string resourcesOriginDirPath = Path.Combine(itemTemplateTempPath, "Resources");
      string resourcesDestinationDirPath = Path.Combine(Path.GetDirectoryName(vsProj.FullName), "Resources");
      if (!Directory.Exists(resourcesDestinationDirPath))
      {
        resourcesFolder = vsProj.ProjectItems.AddFolder("Resources");
      }
      else
      {
        resourcesFolder = vsProj.ProjectItems.Item("Resources");
      }

      foreach (string resourceFile in resourcesFiles)
      {
        var resourcesFilesOriginPath = Path.Combine(resourcesOriginDirPath, resourceFile);
        var resourcesFilesDestinationPath = Path.Combine(resourcesDestinationDirPath, resourceFile);
        if (File.Exists(resourcesFilesDestinationPath))
        {
          continue;
        }

        File.Copy(resourcesFilesOriginPath, resourcesFilesDestinationPath, true);
        resourcesFolder.ProjectItems.AddFromFile(resourcesFilesOriginPath);
      }
    }

    /// <summary>
    /// Gets the visual studio version.
    /// </summary>
    /// <param name="_dte">The DTE object.</param>
    /// <returns></returns>
    protected internal static string GetVisualStudioVersion(DTE _dte)
    {
#if NET_40_OR_GREATER
      return _dte != null ? _dte.Version : null;
#else
      return "9.0";
#endif
    }

    /// <summary>
    /// Enumerate the type of supported projects for the Item Templates wizard.
    /// </summary>
    public enum ProjectWizardType : int
    {
      AspNetMVC = 1,
      WindowsForms = 2
    };
  }
}
