using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Extensions;
using Fasterflect;
using FastMember;

namespace ReflectionSpeed {
    [RankColumn, MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [PlainExporter, RPlotExporter]
    [SimpleJob(invocationCount:1, runStrategy:BenchmarkDotNet.Engines.RunStrategy.Monitoring)]
    public class CreateGetterOverhead {

        //[Params(0, 1, 10_000, 100_000, 10_000_000)]
        [Params(1000)]
        public int CollectionSize;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Verify<T>(T expected, object actual) where T : IEquatable<T> {
            if (!(actual is T a) || !EqualityComparer<T>.Default.Equals(expected, a)) {
                throw new Exception($"{expected} != {actual}");
            }
        }

        [Benchmark(Baseline = true)]
        public void Direct() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            
            for (var i = 0; i < classCollection.Count; i++) {
                var row = classCollection[i];
                var val = row.Nr;
                Verify(i, val);
            }

            for (var i = 0; i < anonCollection.Count; i++) {
                var row = classCollection[i];
                var val = row.Nr;
                Verify(i, val);
            }
        }

        [Benchmark]
        public void Fasterflect() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            MemberGetter Getter<T>(IEnumerable<T> coll) {
                return typeof(T).DelegateForGetPropertyValue("Nr");
            }

            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter(classCollection[i]);
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter(anonCollection[i]);
                Verify(i, val);
            }
        }

        [Benchmark]
        public void Reflection() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            Func<object, object> Getter<T>(IEnumerable<T> coll) {
                var ti = typeof(T).GetProperty("Nr");
                if(ti == null) throw new Exception();
                return ti.GetValue;
            }

            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter(classCollection[i]);
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter(anonCollection[i]);
                Verify(i, val);
            }
        }

        [Benchmark]
        public void OpenDelegate() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            Func<object, object> Getter<T>(IEnumerable<T> coll) {
                var ti = typeof(T).GetProperty("Nr");
                if(ti == null) throw new Exception();
                return ti.GetGetMethodAsOpenDelegate();
            }

            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter(classCollection[i]);
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter(anonCollection[i]);
                Verify(i, val);
            }
        }

        [Benchmark]
        public void FastReflection() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            MemberGetter<object, object> Getter<T>(IEnumerable<T> coll) {
                var ti = typeof(T).GetProperty("Nr");
                if(ti == null) throw new Exception();
                return ti.DelegateForGet();
            }

            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter(classCollection[i]);
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter(anonCollection[i]);
                Verify(i, val);
            }
        }

        [Benchmark]
        public void FastMember() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            TypeAccessor Getter<T>(IEnumerable<T> coll) => TypeAccessor.Create(typeof(T));
            
            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter[classCollection[i], "Nr"];
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter[anonCollection[i], "Nr"];
                Verify(i, val);
            }
        }


        [Benchmark]
        public void Lambda() {
            var classCollection = Enumerable.Range(0, CollectionSize).Select(i => new DataClass {Nr = i}).ToList();
            var anonCollection = Enumerable.Range(0, CollectionSize).Select(i => new {Nr = i}).ToList();

            Func<T, object> Getter<T>(IEnumerable<T> coll) => EnumerableExtensions.GetGetFunc<T>("Nr");

            var classGetter = Getter(classCollection);
            for (var i = 0; i < classCollection.Count; i++) {
                var val = classGetter(classCollection[i]);
                Verify(i, val);
            }

            var anonGetter = Getter(anonCollection);
            for (var i = 0; i < anonCollection.Count; i++) {
                var val = anonGetter(anonCollection[i]);
                Verify(i, val);
            }
        }

    }
}
