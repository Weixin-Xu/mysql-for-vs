﻿// Copyright © 2013, 2018, Oracle and/or its affiliates. All rights reserved.
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
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Xunit;

namespace MySql.Parser.Tests
{
  
  public class AccountManagement
  {
    [Fact]
    public void CreateUser1()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY 'mypass';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void CreateUser2()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void CreateUser3()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost'
IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void CreateUser4()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost'
IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689', 'me'@'localhost'
IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void CreateUser()
    {
      // Test all auth plugins.
      MySQL51Parser.program_return r = null;
      r = ParseSqlFor80(@"CREATE USER IF NOT EXISTS 'rootnative'@'localhost'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH mysql_native_password BY 'guidev!'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH sha256_password BY 'guidev!'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH caching_sha2_password BY 'guidev!'");
      Assert.Throws<Xunit.Sdk.EqualException>(() => ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH caching_sha2_passwords BY 'guidev!'"));

      // Other allowed syntax.
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED BY 'guidev!'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED BY PASSWORD 'guidev!'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH caching_sha2_password");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH mysql_native_password BY 'guidev!'");
      r = ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED WITH mysql_native_password AS 'guidev!'");

      // Non-allowed syntax.
      Assert.Throws<Xunit.Sdk.EqualException>(() => ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' IDENTIFIED BY caching_sha2_passwords WITH 'guidev!'"));
      Assert.Throws<Xunit.Sdk.EqualException>(() => ParseSqlFor80(@"CREATE USER 'rootnative'@'localhost' WITH caching_sha2_passwords"));

      // Variants.
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY 'new_password' PASSWORD EXPIRE DEFAULT");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY 'new_password' PASSWORD EXPIRE");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' IDENTIFIED WITH sha256_password BY 'new_password' PASSWORD EXPIRE INTERVAL 180 DAY");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' IDENTIFIED WITH mysql_native_password BY 'new_password1', 'jeanne'@'localhost' IDENTIFIED WITH sha256_password BY 'new_password2' REQUIRE X509 WITH MAX_QUERIES_PER_HOUR 60 ACCOUNT LOCK");
      r = ParseSqlFor80(@"CREATE USER 'joe'@'10.0.0.1' DEFAULT ROLE 'administrator', 'developer'");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' REQUIRE NONE");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' REQUIRE SSL");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' REQUIRE ISSUER '/C=SE/ST=Stockholm/L=Stockholm/O=MySQL/CN=CA/emailAddress=ca@example.com'");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' REQUIRE CIPHER 'EDH-RSA-DES-CBC3-SHA'");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' REQUIRE SUBJECT '/C=SE/ST=Stockholm/L=Stockholm/O=MySQL demo client certificate/CN=client/emailAddress=client@example.com' AND ISSUER '/C=SE/ST=Stockholm/L=Stockholm/O=MySQL/CN=CA/emailAddress=ca@example.com' AND CIPHER 'EDH-RSA-DES-CBC3-SHA'");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' WITH MAX_QUERIES_PER_HOUR 500 MAX_UPDATES_PER_HOUR 100");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' PASSWORD HISTORY 6");
      r = ParseSqlFor80(@"CREATE USER 'jeffrey'@'localhost' PASSWORD REUSE INTERVAL 360 DAY");
    }

    public MySQL51Parser.program_return ParseSqlFor80(string sql)
    {
      StringBuilder sb = null;
      return Utility.ParseSql(sql, false, out sb, new Version(8,0));
    }
    
    [Fact]
    public void DropUser()
    {
      string sql = @"DROP USER 'jeffrey'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void DropUser2()
    {
      string sql = @"DROP USER 'jeffrey'@'localhost', 'me'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant()
    {
      string sql = @"GRANT ALL ON db1.* TO 'jeffrey'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant2()
    {
      string sql = @"GRANT SELECT ON db2.invoice TO 'jeffrey'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant3()
    {
      string sql = @"GRANT USAGE ON *.* TO 'jeffrey'@'localhost' WITH MAX_QUERIES_PER_HOUR 90;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant4()
    {
      string sql = @"GRANT ALL ON *.* TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant5()
    {
      string sql = @"GRANT SELECT, INSERT ON *.* TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant6()
    {
      string sql = @"GRANT ALL ON mydb.* TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant7()
    {
      string sql = @"GRANT SELECT, INSERT ON mydb.* TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant8()
    {
      string sql = @"GRANT SELECT (col1), INSERT (col1,col2) ON mydb.mytbl TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant9()
    {
      string sql = @"GRANT CREATE ROUTINE ON mydb.* TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant10()
    {
      string sql = @"GRANT EXECUTE ON PROCEDURE mydb.myproc TO 'someuser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant11()
    {
      string sql = @"GRANT ALL ON test.* TO ''@'localhost'";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant12()
    {
      string sql = @"GRANT USAGE ON *.* TO ''@'localhost' WITH MAX_QUERIES_PER_HOUR 500 MAX_UPDATES_PER_HOUR 100;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant13()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost' IDENTIFIED BY 'goodsecret' REQUIRE SSL;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant14()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret' REQUIRE X509;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant15()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE ISSUER '/C=FI/ST=Some-State/L=Helsinki/
    O=MySQL Finland AB/CN=Tonu Samuel/emailAddress=tonu@example.com';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant16()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE SUBJECT '/C=EE/ST=Some-State/L=Tallinn/
    O=MySQL demo client certificate/
    CN=Tonu Samuel/emailAddress=tonu@example.com';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant17()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE CIPHER 'EDH-RSA-DES-CBC3-SHA';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant18()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE SUBJECT '/C=EE/ST=Some-State/L=Tallinn/O=MySQL demo client certificate/
    CN=Tonu Samuel/emailAddress=tonu@example.com'
  AND ISSUER '/C=FI/ST=Some-State/L=Helsinki/O=MySQL Finland AB/CN=Tonu Samuel/emailAddress=tonu@example.com'
  AND CIPHER 'EDH-RSA-DES-CBC3-SHA';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant19()
    {
      string sql = @"GRANT REPLICATION CLIENT ON *.* TO 'user'@'10.10.10.%'";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Grant20()
    {
      string sql = @"GRANT USAGE ON *.* TO 'bob'@'%.loc.gov' IDENTIFIED BY 'newpass';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void GrantProxy51()
    {
      string sql = @"GRANT PROXY ON 'localuser'@'localhost' TO 'externaluser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, true, out sb, new Version( 5, 1 ));
      Assert.True(sb.ToString().IndexOf("no viable alternative at input 'GRANT'") != -1);
    }

    [Fact]
    public void GrantProxy55()
    {
      string sql = @"GRANT PROXY ON 'localuser'@'localhost' TO 'externaluser'@'somehost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb, new Version( 5, 5 ));
    }

    [Fact]
    public void Rename()
    {
      string sql = "RENAME USER 'jeffrey'@'localhost' TO 'jeff'@'127.0.0.1';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Revoke()
    {
      string sql = "REVOKE INSERT ON *.* FROM 'jeffrey'@'localhost';";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void Revoke2()
    {
      string sql = "REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'jeffrey'@'localhost', 'jeff'@'127.0.0.1', 'me'@'localhost'";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Fact]
    public void RevokeProxy51()
    {
      string sql = "REVOKE PROXY ON 'jeffrey'@'localhost' FROM 'jeff'@'127.0.0.1', 'me'@'localhost'";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, true, out sb, new Version( 5, 1 ));
      Assert.True(sb.ToString().IndexOf("no viable alternative at input 'REVOKE'") != -1);
    }

    [Fact]
    public void RevokeProxy55()
    {
      string sql = "REVOKE PROXY ON 'jeffrey'@'localhost' FROM 'jeff'@'127.0.0.1', 'me'@'localhost'";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb, new Version( 5, 5 ));
    }

    [Fact]
    public void SetPassword()
    {
      string sql = "SET PASSWORD FOR 'bob'@'%.loc.gov' = PASSWORD('newpass');";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }
  }
}
