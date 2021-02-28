using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Queues;

namespace DedupSharp.Stages
{
    public static class FragmentStage
    {
        public static void FragIntoSingleNumbers(BlockingCollection<int[][]> input, BlockingCollection<int[]> output)
        {
            try
            {
                foreach (var item in input.GetConsumingEnumerable())
                    foreach (var arr in item)
                        output.Add(SortOne(arr));
            }
            finally
            {
                output.CompleteAdding();
            }
        }

        public static int[] SortOne(int[] array)
        {
            Array.Sort(array);
            return array;
        }
    }
}