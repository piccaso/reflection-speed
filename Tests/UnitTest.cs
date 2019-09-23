using System;
using System.Collections.ObjectModel;
using System.Linq;
using Extensions;
using NUnit.Framework;
using ReflectionSpeed;

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
            Assert.IsTrue(dictionaries[0].ContainsKey("Nr"));
            Assert.IsTrue(dictionaries[0].ContainsKey("Text"));
            Assert.IsTrue(dictionaries[0].ContainsKey("Nullable"));
            Assert.AreEqual("1", dictionaries[0]["Text"]);
            Assert.AreEqual(1, dictionaries[0]["Nr"]);
        }

        [Test]
        public void SomeTypeToDataTable() {
            var dt = Enumerable.Range(1, 1000).Select(r => new SomeType {Nr = r, Text = r.ToString(), Nullable = r % 2 == 0 ? r / 2 : (int?) null}).ToDataTable();
            Assert.IsNotNull(dt);
            Assert.AreEqual(1000, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0].ItemArray[0]);
            Assert.AreEqual("1", dt.Rows[0].ItemArray[1]);
            Assert.AreEqual(DBNull.Value, dt.Rows[0].ItemArray[2]);
            Assert.AreEqual(1, dt.Rows[1].ItemArray[2]);
        }

        [Test]
        public void DynamicToDict() {
            var dictionaries = Enumerable.Range(1, 1000).Select(r => new {Nr = r, Text = r.ToString(), Nullable = r % 2 == 0 ? r / 2 : (int?) null}).ToDictionaryCollection().ToList();
            Assert.IsNotNull(dictionaries);
            Assert.AreEqual(1000, dictionaries.Count);
            Assert.IsTrue(dictionaries[0].ContainsKey("Nr"));
            Assert.IsTrue(dictionaries[0].ContainsKey("Text"));
            Assert.IsTrue(dictionaries[0].ContainsKey("Nullable"));
            Assert.AreEqual("1", dictionaries[0]["Text"]);
            Assert.AreEqual(1, dictionaries[0]["Nr"]);
        }

        [Test]
        public void CompileTypedSetter() {
            var o = new SomeType();
            var setter = EnumerableExtensions.GetSetAction<SomeType, int?>("Nullable");

            setter(o, 1);
            Assert.AreEqual(1, o.Nullable);
            Assert.IsTrue(o.Nullable.HasValue);

            setter(o, null);
            Assert.AreEqual(null, o.Nullable);
            Assert.IsFalse(o.Nullable.HasValue);
        }

        
        [Test]
        public void CompileSetter() {
            var o = new SomeType();
            var setter = EnumerableExtensions.GetSetAction<SomeType>("Nullable");

            setter(o, 1);
            Assert.AreEqual(1, o.Nullable);
            Assert.IsTrue(o.Nullable.HasValue);

            setter(o, null);
            Assert.AreEqual(null, o.Nullable);
            Assert.IsFalse(o.Nullable.HasValue);
        }

        //[Test]
        public void EmitGetter() {
            var o = new SomeType();
            var pi = typeof(SomeType).GetProperty("Nullable");
            var getter = IlEmit.EmitGetter(pi); // not working!
            Assert.AreEqual(null, getter(o));
            o.Nullable = 3;
            Assert.AreEqual(3, getter(o));

        }
    }

    public class SomeType {
        public int Nr { get; set; }
        public string Text { get; set; }
        public int? Nullable { get; set; }
    }
}