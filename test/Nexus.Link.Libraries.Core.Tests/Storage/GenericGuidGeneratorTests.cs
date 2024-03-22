using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

namespace Nexus.Link.Libraries.Core.Tests.Storage
{
    [TestClass]
    public class GenericGuidGeneratorTests
    {
        [TestMethod]
        [DataRow(SequentialGuidType.AsBinary)]
        [DataRow(SequentialGuidType.AsString)]
        [DataRow(SequentialGuidType.AtEnd)]
        public void Should_generate_unique_Guid(SequentialGuidType sequentialGuidType)
        {
            var guidGenerator = new GenericGuidGenerator(sequentialGuidType);

            const int numberOfGuidsToCreate = 100000;
            var list = new List<Guid>(numberOfGuidsToCreate);

            for (var i = 0; i < numberOfGuidsToCreate; i++)
            {
                list.Add(guidGenerator.NewGuid());
            }

            var duplicatesExists = list.GroupBy(g => g)
                .Select(g => new
                {
                    Count = g.Count(),
                    Guid = g.Key
                })
                .Any(x => x.Count > 1);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(duplicatesExists);
            for (var i = 0; i < list.Count - 1; i++)
            {
                Console.WriteLine(list[i]);
            }
        }

        [TestMethod]
        [DataRow(SequentialGuidType.AsBinary)]
        [DataRow(SequentialGuidType.AsString)]
        [DataRow(SequentialGuidType.AtEnd)]
        public void Should_be_completely_thread_safe_to_avoid_duplicates(SequentialGuidType sequentialGuidType)
        {
            var guidGenerator = new GenericGuidGenerator(sequentialGuidType);

            var timer = Stopwatch.StartNew();

            var threadCount = 20;

            var loopCount = 1024; // Change to 1024 * 1024 when you want to test this more extensively;

            var limit = loopCount * threadCount;

            var ids = new Guid[limit];

            ParallelEnumerable
                .Range(0, limit)
                .WithDegreeOfParallelism(8)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(x =>
                {
                    ids[x] = guidGenerator.NewGuid();
                });

            timer.Stop();

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit / timer.ElapsedMilliseconds);

            Console.WriteLine("Distinct: {0}", ids.Distinct().Count());

            IGrouping<Guid, Guid>[] duplicates = ids.GroupBy(x => x).Where(x => x.Count() > 1).ToArray();

            Console.WriteLine("Duplicates: {0}", duplicates.Count());

            foreach (IGrouping<Guid, Guid> newId in duplicates)
                Console.WriteLine("{0} {1}", newId.Key, newId.Count());

        }

        [TestMethod]
        [DataRow(SequentialGuidType.AsBinary)]
        [DataRow(SequentialGuidType.AsString)]
        [DataRow(SequentialGuidType.AtEnd)]
        public void Should_generate_sequential_ids_quickly(SequentialGuidType sequentialGuidType)
        {
            var guidGenerator = new GenericGuidGenerator(sequentialGuidType);

            var limit = 10;

            var ids = new Guid[limit];
            for (var i = 0; i < limit; i++)
            {
                ids[i] = guidGenerator.NewGuid();
            }

            for (var i = 0; i < limit - 1; i++)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(ids[i], ids[i + 1]);
                Console.WriteLine(ids[i]);
            }
        }
    }
}