using NUnit.Framework;
using Microsoft.Data.Sqlite;
using System.IO;
using AutocodeDB.Helpers;
using System;

namespace SqlDataInsert.Tests
{
    [TestFixture]
    public class SqlTaskTests
    {
        private const string QueriesFileName = "insert.sql";
        private static readonly string QueriesFile = SqlTask.GetQueriesFullPath(QueriesFileName);
        private static readonly string[] Queries = QueryHelper.GetQueries(QueriesFile);

        [OneTimeSetUp]
        public void Setup()
        {
            SqliteHelper.OpenConnection("./DB/supermarket.db");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            SqliteHelper.CloseConnection();
        }

        #region Local Tests

        [Order(1)]
        [Test]
        public void FileWithQueries_Exists()
        {
            var actual = File.Exists(QueriesFile);
            var message = $"Couldn't find the '{QueriesFileName}' file.";
            Assert.IsTrue(actual, message);

        }

        [Order(2)]
        [Test]
        public void FileWithQueries_NotEmpty()
        {
            var message = $"The file '{QueriesFileName}' contains no entries.";
            Assert.IsNotEmpty(Queries, message);

        }

        [Order(3)]
        [TestCaseSource(nameof(Queries))]
        public void InsertQueryString_ContainsCorrectInsertInstruction(string query)
        {
            var actual = InsertHelper.ContainsCorrectInsertInstruction(query);
            var message = QueryHelper.ComposeErrorMessage(query, "The query doesn't contain 'INSERT INTO' instruction.");
            Assert.IsTrue(actual, message);
        }

        [Order(4)]
        [TestCaseSource(nameof(Queries))]
        public void AllInsertQueries_ExecuteSuccessfully(string query)
        {
            try
            {
                var command = new SqliteCommand(query, SqliteHelper.Connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Assert.Fail(QueryHelper.ComposeErrorMessage(query, e, "Query execution caused an exception."));
            }
        }

        #endregion
    }
}