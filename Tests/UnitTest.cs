using System;
using System.Linq;
using Extensions;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void AnonTypeToDictionaryCollection() {
            var dictionaries = Enumerable.Range(1, 1000).Select(r => new {Nr = r, Text = r.ToString()}).ToDictionaryCollection().ToList();
            Assert.IsNotNull(dictionaries);
            Assert.AreEqual(1000, dictionaries.Count);
            Assert.AreEqual("Nr", dictionaries[0].Keys.First());
            Assert.AreEqual("1", dictionaries[0]["Text"]);
            Assert.AreEqual(1, dictionaries[0]["Nr"]);
        }

        [Test]
        public void AnonTypeToDataTable() {
            var dt = Enumerable.Range(1, 1000).Select(r => new {Nr = r, Text = r.ToString()}).ToDataTable();
            Assert.IsNotNull(dt);
            Assert.AreEqual(1000, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);
        }

        [Test]
        public void SomeTypeToDictionaryCollection() {
            var dictionaries = Enumerable.Range(1, 1000).Select(r => new SomeType {Nr = r, Text = r.ToString(), Nullable = r % 2 == 0 ? r / 2 : (int?) null}).ToDictionaryCollection().ToList();
            Assert.IsNotNull(dictionaries);
            Assert.AreEqual(1000, dictionaries.Count);
            Assert.AreEqual("Nr", dictionaries[0].Keys.First());
            Assert.AreEqual("1", dictionaries[0]["Text"]);
            Assert.AreEqual(1, dictionaries[0]["Nr"]);
        }

        [Test]
        public void SomeTypeToDataTable() {
            var dt = Enumerable.Range(1, 1000).Select(r => new SomeType {Nr = r, Text = r.ToString(), Nullable = r % 2 == 0 ? r / 2 : (int?) null}).ToDataTable();
            Assert.IsNotNull(dt);
            Assert.AreEqual(1000, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);
            Assert.AreEqual(DBNull.Value, dt.Rows[0][2]);
            Assert.AreEqual(1, dt.Rows[1][2]);
        }
    }

    public class SomeType {
        public int Nr { get; set; }
        public string Text { get; set; }
        public int? Nullable { get; set; }
    }
}