using NUnit.Framework;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using AutocodeDB.Models;
using AutocodeDB.Helpers;

namespace AlterTable.Tests
{
    [TestFixture]
    public class AlterTableTasksTests
    {
        private const int FilesCount = 3;
        private static readonly string[] DMLTargetTables = { "manufacturer", "person", "supermarket" };
        private readonly string DatabaseFile;
        private readonly string EmptyDatabaseFile;
        private readonly string InsertFile;
        private readonly string[] FileNames;
        private readonly string[] QueryFiles;
        private readonly string[] Queries;
        private SelectResult[] ActualResults;
        private SelectResult[] ExpectedResults;

        public AlterTableTasksTests()
        {
            FileIOHelper.FilesCount = FilesCount;
            FileIOHelper.GenerateProjectDirectory(Environment.CurrentDirectory);
            DatabaseFile = FileIOHelper.GetDBFullPath("marketplace.db");
            EmptyDatabaseFile = FileIOHelper.GetEmptyDBFullPath("empty_tables.db");
            InsertFile = FileIOHelper.GetInsertFileFullPath("insert.sql");
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

        #region Workload
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
            AssertFileExist(index - 1);
        }

        [Order(2)]
        [Test]
        public void FileWithQueries_NotEmpty([Range(1, FilesCount)] int index)
        {
            AssertFileNotEmpty(index - 1);
        }

        [Order(3)]
        [Test]
        public void AlterQuery_ContainsAlterTableAddColumn([Range(1, FilesCount)] int index)
        {
            --index;
            var actual = Queries[index];
            Assert.IsTrue(AlterTableHelper.ContainsAddColumn(actual), "Query should contain correct ALTER TABLE ADD COLUMN statement.");
        }

        [Order(4)]
        [Test]
        public void AllAlterQueries_ExecuteSuccessfully([Range(1, FilesCount)] int index)
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

        [Order(5)]
        [Test]
        public void AlterQuery_ReturnsCorrectSchema([Range(1, FilesCount)] int index)
        {
            --index;
            AssertData(index);
            var expected = ExpectedResults[index].Schema;
            var actual = ActualResults[index].Schema;
            var expectedMessage = MessageComposer.Compose(expected);
            var actualMessage = MessageComposer.Compose(actual);
            Assert.AreEqual(expected, actual, "\nExpected:\n{0}\n\nActual:\n{1}\n", expectedMessage, actualMessage);
        }

        [Order(6)]
        [Test]
        public void AlterQuery_ReturnsCorrectTypes([Range(1, FilesCount)] int index)
        {
            --index;
            AssertData(index);
            var expected = ExpectedResults[index].Types;
            var actual = ActualResults[index].Types;
            var expectedMessage = MessageComposer.Compose(expected);
            var actualMessage = MessageComposer.Compose(actual);
            Assert.AreEqual(expected, actual, "\nExpected:\n{0}\n\nActual:\n{1}\n", expectedMessage, actualMessage);
        }

        [Order(7)]
        [Test]
        public void AlterQuery_ReturnsCorrectData([Range(1, FilesCount)] int index)
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

        #region Private methods

        private void AssertData(int index)
        {
            AssertFileExist(index);
            AssertFileNotEmpty(index);
            AssertErrors(index);
        }

        private void AssertErrors(int index)
        {
            if (!string.IsNullOrEmpty(ActualResults[index].ErrorMessage))
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

        #endregion
    }
}