using System;
using System.IO;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using AutocodeDB.Helpers;
using AutocodeDB.Models;

namespace SimpleUpdate.Tests
{
    [TestFixture]
    public class SqlTaskTests
    {
        private const int FilesCount = 4;
        private readonly string[] DMLTargetTables = { "supermarket", "product", "customer", "product" };
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
            ExpectedResults=FileIOHelper.DeserializeResultFiles(FileNames);
        }

        /// Comment this region if you want fill DataBase fro sql file
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
        public void UpdateQuery_ContainsUpdateSetFrom([Range(1, FilesCount)] int index)
        {
            --index;
            var actual = Queries[index];
            Assert.IsTrue(UpdateHelper.ContainsCorrectUpdateSetInstruction(actual), "Query should contain 'UPDATE' and 'SET' statements.");
        }

        [Order(4)]
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void UpdateQuery_ContainsUpdateSetWhereFrom(int index)
        {
            --index;
            var actual = Queries[index];
            Assert.IsTrue(UpdateHelper.ContainsCorrectUpdateSetWhereInstruction(actual), "Query should contain 'UPDATE', 'SET' and 'WHERE' statements.");
        }

//        [Order(5)]
//        [Test]
//        [TestCase(3)]
//        public void UpdateQuery_ContainsUpdateSetWhereSubselectFrom(int index)
//        {
//            --index;
//            var actual = Queries[index];
//            Assert.IsTrue(UpdateHelper.ContainsCorrectUpdateSetWhereSubselectInstruction(actual), "Query should contain 'UPDATE', 'SET', 'WHERE' statements and subselect inside");
//        }

        [Order(6)]
        [Test]
        public void UpdateQuery_ExecutesSuccessfully([Range(1, FilesCount)] int index)
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

        [Order(7)]
        [Test]
        public void UpdateQuery_ReturnsCorrectData([Range(1, FilesCount)] int index)
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

        //[Order(8)]
        //[Test]
        //public void SelectQuery_ReturnsCorrectRowsCount([Range(1, FilesCount)] int index)
        //{
        //    --index;
        //    AssertData(index);
        //    var expected = ExpectedResults[index].Data.Length;
        //    var actual = ActualResults[index].Data.Length;
        //    Assert.AreEqual(expected, actual, ActualResults[index].ErrorMessage);
        //}

        //[Order(9)]
        //[Test]
        //public void SelectQuery_ReturnsCorrectSchema([Range(1, FilesCount)] int index)
        //{
        //    --index;
        //    AssertData(index);
        //    var expected = ExpectedResults[index].Schema;
        //    var actual = ActualResults[index].Schema;
        //    var expectedMessage = MessageComposer.Compose(expected);
        //    var actualMessage = MessageComposer.Compose(actual);
        //    Assert.AreEqual(expected, actual, "\nExpected:\n{0}\n\nActual:\n{1}\n", expectedMessage, actualMessage);
        //}

        //[Order(10)]
        //[Test]
        //public void SelectQuery_ReturnsCorrectTypes([Range(1, FilesCount)] int index)
        //{
        //    --index;
        //    AssertData(index);
        //    var expected = ExpectedResults[index].Types;
        //    var actual = ActualResults[index].Types;
        //    var expectedMessage = MessageComposer.Compose(expected);
        //    var actualMessage = MessageComposer.Compose(actual);
        //    Assert.AreEqual(expected, actual, "\nExpected:\n{0}\n\nActual:\n{1}\n", expectedMessage, actualMessage);
        //}

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
            Console.WriteLine($"äctual={actual}");
            if (string.IsNullOrWhiteSpace(actual))
                Assert.Fail(message);
        }

    }
}