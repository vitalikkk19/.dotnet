using System;
using System.IO;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SimpleDelete;
using AutocodeDB.Helpers;
using AutocodeDB.Models;
using AutocodeDB.Helpers;

namespace SimpleUpdate.Tests
{
    [TestFixture]
    public class SqlTaskTests
    {
        private const int FilesCount = 3;
        private readonly string[] DMLTargetTables = { "contact_type", "customer_order", "customer_order" };
        private readonly string DatabaseFile;
        private readonly string EmptyDatabaseFile;
        private readonly string InsertFile;
        private readonly string[] FileNames;
        private readonly string[] QueryFiles;
        private readonly string[] Queries;
        private SelectResult[] ActualResults;
        private SelectResult[] ExpectedResults;

        public SqlTaskTests()
        {
            FileIOHelper.FilesCount = FilesCount;
            FileIOHelper.GenerateProjectDirectory(Environment.CurrentDirectory);
            DatabaseFile = FileIOHelper.GetDBFullPath("marketplace.db");
            EmptyDatabaseFile = FileIOHelper.GetEmptyDBFullPath("empty_tables.db");
            InsertFile = FileIOHelper.GetInsertFileFullPath("insert_for_delete.sql");
            FileNames = FileIOHelper.GetFilesNames();
            QueryFiles = SqlTask.GetFilePaths(FileNames);
            Queries = QueryHelper.GetQueries(QueryFiles);
        }


        [OneTimeSetUp]
        public void Setup()
        {
            ActualResults = new SelectResult[FilesCount];
            ExpectedResults = new SelectResult[FilesCount];
            ExpectedResults = FileIOHelper.DeserializeResultFiles(FileNames);
        }

        #region WorkLoad
        [SetUp]
        public void LocalSetup()
        {
            SqliteHelper.OpenConnection(DatabaseFile);
        }
        [TearDown]
        public void LocalCleanup()
        {
            SqliteHelper.CloseConnection();
        }
        #endregion

        [Order(1)]
        [Test]
        public void FileWithQueries_Exists([Range(1, FilesCount)] int index)
        {
            --index;
            AssertFileExist(index);
        }

        [Order(2)]
        [Test]
        public void FileWithQueries_NotEmpty([Range(1, FilesCount)] int index)
        {
            --index;
            AssertFileNotEmpty(index);
        }


        [Order(3)]
        [Test]
        public void DeleteQuery_ContainsDeleteFrom([Range(1, FilesCount)] int index)
        {
            --index;
            var actual = Queries[index];
            Assert.IsTrue(DeleteHelper.ContainsDeleteFrom(actual), "Query should contain 'DELETE FROM' statement.");
        }


        [Order(4)]
        [Test]
        public void DeleteQuery_ContainsDeleteFromWhere([Range(1, FilesCount)] int index)
        {
            --index;
            var actual = Queries[index];
            Assert.IsTrue(DeleteHelper.ContainsDeleteFromWhere(actual), "Query should contain 'DELETE FROM' and 'WHERE' statements.");
        }

        [Order(5)]
        [Test]
        public void DeleteQuery_ExecutesSuccessfully([Range(1, FilesCount)] int index)
        {
            --index;
            var actual = Queries[index];
            try
            {
                var command = new SqliteCommand(actual, SqliteHelper.Connection);
                command.ExecuteNonQuery();
                ActualResults[index] = SelectHelper.DumpTable(DMLTargetTables[index]);
            }
            catch (SqliteException exception)
            {
                var message = QueryHelper.ComposeErrorMessage(actual, exception, "Query execution caused an exception.");
                Assert.Fail(message);
            }
        }

        [Order(6)]
        [Test]
        public void DeleteQuery_MakesCorrectData([Range(1, FilesCount)] int index)
        {
            Console.WriteLine(FileNames);
            --index;
            AssertData(index);
            Console.WriteLine(ExpectedResults[index]);
            var expected = ExpectedResults[index].Data;
            var actual = ActualResults[index].Data;
            var expectedMessage = MessageComposer.Compose(ExpectedResults[index].Schema, expected);
            var actualMessage = MessageComposer.Compose(ActualResults[index].Schema, actual);
            Assert.AreEqual(expected, actual, "\nExpected:\n{0}\n\nActual:\n{1}\n", expectedMessage, actualMessage);
        }

        private void AssertData(int index)
        {
            AssertFileExist(index);
            AssertFileNotEmpty(index);
            AssertErrors(index);
        }

        private void AssertErrors(int index)
        {
            if (ActualResults[index] != null && !string.IsNullOrEmpty(ActualResults[index].ErrorMessage))
                Assert.Fail(ActualResults[index].ErrorMessage);
        }

        private void AssertFileExist(int index)
        {
            var actual = Queries[index];
            var message = $"The file '{FileNames[index]}' was not found.";
            if (actual == null)
                Assert.Fail(message);
        }

        private void AssertFileNotEmpty(int index)
        {
            var actual = Queries[index];
            var message = $"The file '{FileNames[index]}' contains no entries.";
            if (string.IsNullOrWhiteSpace(actual))
                Assert.Fail(message);
        }

    }
}