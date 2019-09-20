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
        public void Test1() {
            var dictionaries = Enumerable.Range(1, 1000).Select(r => new {Nr = r, Text = r.ToString()}).ToDictionaryCollection();
            Assert.IsNotNull(dictionaries);
            Assert.AreEqual(1000, dictionaries.Count);
        }

        [Test]
        public void Test2() {
            var dt = Enumerable.Range(1, 1000).Select(r => new {Nr = r, Text = r.ToString()}).ToDataTable();
            Assert.IsNotNull(dt);
            Assert.AreEqual(1000, dt.Rows.Count);
        }
    }
}