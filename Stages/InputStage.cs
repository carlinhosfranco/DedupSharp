using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Helpers.IO;

namespace DedupSharp.Stages
{
    public sealed class InputStage<TOutput> : ISourceBlock<TOutput>
    {
        public BufferBlock<TOutput> BufferStage { get; private set; } = new BufferBlock<TOutput>();
        private const int WS = 10;

        public Task Completion => throw new NotImplementedException();

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            throw new NotImplementedException();
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void FillInternalBuffer(IEnumerable<TOutput> bufferItems)
        {
            foreach (var item in bufferItems)
                BufferStage.Post(item);            

            BufferStage.Complete();
        }

        public void FillInternalBuffer(TOutput bufferItem)
        {
            BufferStage.Post(bufferItem);
            //BufferStage.Complete();
        }
            

        // public BufferBlock<T[]> CreateQueue<T>()
        // {
        //     var source = new BufferBlock<T[]>();
        //     var queue = new Queue<T>();

        //     var target = new ActionBlock<T>(item =>
        //     {
        //         // Add the item to the queue.
        //         queue.Enqueue(item);
        //         // Remove the oldest item when the queue size exceeds the window size.
        //         if (queue.Count > WS)
        //             queue.Dequeue();

        //         // Post the data in the queue to the source block when the queue size
        //         // equals the window size.
        //         if (queue.Count == WS)
        //             source.Post(queue.ToArray());
        //     });

        //     target.Completion.ContinueWith(delegate {
        //         if (queue.Count > 0 && queue.Count < WS)
        //             source.Post(queue.ToArray());

        //         source.Complete();
        //     });

        //     return source;
        // }

        // public void Process(ITargetBlock<int[]> target)
        // {            
        //     var num = GetNumbers();            
        //     target.Post(num);
        //     target.Complete();
        //     target.Completion.Wait();
        // }

        // public int[] GetNumbers()
        // {
        //     _input = new int[WS];

        //     for (int i = 0; i < WS; i++)
        //         _input[i] = _ran.Next(1, 500);

        //     return _input;            
        // }
        // public TransformBlock<double[], double[]> Que()
        // {
        //     return new TransformBlock<double[], double[]>( q => {
        //                     for (int i = 0; i < q.Length; i++)
        //                         Console.WriteLine($"Source: {q[i]}\n");
        //                     return _input;
        //                 });
        // }

        // public void ReadFilesFromDisk( string path, ref ITargetBlock<FileStream> queue)
        // {
        //     string[] files = ReadFiles.Get(path);

        //     if (files != null)
        //     {
        //         for (int i = 0; i < files.Length; i++)
        //         {
        //             FileStream file = File.Open(files[i], FileMode.Open, FileAccess.Read);                                        
        //             queue.Post(file);
        //         }
        //         queue.Complete();               
        //     }
        // }        
    }
}