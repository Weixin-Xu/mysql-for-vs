// Copyright (c) 2008 MySQL AB, 2008-2010 Sun Microsystems, Inc.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA  

using System;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Tests;
using System.Data.EntityClient;
using System.Data.Common;
using NUnit.Framework;
using System.Data.Objects;
using System.Linq;

namespace MySql.Data.Entity.Tests
{
	[TestFixture]
	public class ProceduresAndFunctions : BaseEdmTest
	{
        public ProceduresAndFunctions()
            : base()
        {
        }

        [Test]
        public void Insert()
        {
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Authors", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            int count = dt.Rows.Count;

            using (testEntities context = new testEntities())
            {
                Author a = new Author();
                a.Id = 23;
                a.Name = "Test name";
                a.Age = 44;
                context.AddToAuthors(a);
                context.SaveChanges();
            }

            dt.Clear();
            da.Fill(dt);
            Assert.AreEqual(count + 1, dt.Rows.Count);
            Assert.AreEqual(23, dt.Rows[count]["id"]);
        }

        [Test]
        public void Update()
        {
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Authors", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            int count = dt.Rows.Count;

            using (testEntities context = new testEntities())
            {
                var q = from a in context.Authors
                        where a.Name == "Don Box"
                        select a;
               foreach (Author a in q)
                   a.Name = "Dummy";
               context.SaveChanges();
            }

            da.SelectCommand.CommandText = "SELECT * FROM Authors WHERE name='Dummy'";
            dt.Clear();
            da.Fill(dt);
            Assert.AreEqual(1, dt.Rows.Count);
        }

        [Test]
        public void Delete()
        {
            using (testEntities context = new testEntities())
            {
                foreach (Book b in context.Books)
                    context.DeleteObject(b);
                context.SaveChanges();
            }

            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Books", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            Assert.AreEqual(0, dt.Rows.Count);
        }

        /// <summary>
        /// Bug #45277	Calling User Defined Function using eSql causes NullReferenceException
        /// </summary>
        [Test]
        public void UserDefinedFunction()
        {
            using (EntityConnection conn = new EntityConnection("name=testEntities"))
            {
                conn.Open();

                string query = @"SELECT e.FirstName AS Name FROM testEntities.Employees AS e 
                    WHERE testModel.Store.spFunc(e.Id, '') = 6";
                using (EntityCommand cmd = new EntityCommand(query, conn))
                {
                    EntityDataReader reader = cmd.ExecuteReader();
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(6, reader[0]);
                }
            }
        }

        /// <summary>
        /// Bug #56806	Default Command Timeout has no effect in connection string
        /// </summary>
        [Test]
        public void CommandTimeout()
        {
            string connectionString = String.Format(
                "metadata=res://*/TestModel.csdl|res://*/TestModel.ssdl|res://*/TestModel.msl;provider=MySql.Data.MySqlClient; provider connection string=\"{0};default command timeout=5\"", GetConnectionString(true));
            EntityConnection connection = new EntityConnection(connectionString);

            using (testEntities context = new testEntities(connection))
            {
                Author a = new Author();
                a.Id = 66;  // special value to indicate the routine should take 30 seconds
                a.Name = "Test name";
                a.Age = 44;
                context.AddToAuthors(a);
                try
                {
                    context.SaveChanges();
                    Assert.Fail("This should have timed out");
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }
        }

    }
}