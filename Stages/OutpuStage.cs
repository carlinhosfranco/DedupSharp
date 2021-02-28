using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Stages
{
    public static class OutpuStage
    {

        public static void Output(BlockingCollection<int[]> output)
        {
            foreach (var item in output.GetConsumingEnumerable())
                Console.WriteLine($" Values: [{string.Join(",", item)}]\n");
        }
    }
}