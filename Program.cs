using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Stages;

namespace DedupSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new InputStage();
            var inputQ = input.CreateQueue<int>();
            inputQ.Post(input.GetNumbers() );

            var outstage = new OutpuStage();
            var outQ = outstage.PrintConsole();

            var fr = new FragmentStage(inputQ, outQ);

            fr.Process();

            var completion = outQ.Completion.ContinueWith(delegate { Console.WriteLine(); });

            completion.Wait();
           
            // var outstage = new OutpuStage();
            // var outQ = outstage.PrintConsole();

            // var fr = new FragmentStage(inputQ, outQ);

            // var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
           
            // inputQ.LinkTo(frQ, linkOptions);
            // frQ.LinkTo(outQ, linkOptions);

            // inputQ.Post(input.GetNumbers());

            // inputQ.Complete();

            // outQ.Completion.Wait();
        }
    }
}
