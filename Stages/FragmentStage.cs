using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Queues;

namespace DedupSharp.Stages
{
    public sealed class FragmentStage<TInput> : ITargetBlock<TInput>
    {
        public Task Completion => throw new NotImplementedException();

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<int>> ConsumeAsync(ISourceBlock<TInput> source)
        {
            var itensProcessed = new List<int>();
            
            while (await source.OutputAvailableAsync())
            {
                var item = await source.ReceiveAsync();
                var r = ChangeType(item);

                itensProcessed.AddRange(r);
            }

            return itensProcessed;
        }

        public async Task<IEnumerable<int>> ConsumeAsync(IReceivableSourceBlock<TInput> source)
        {
            //int bytesProcessed = 0;
            while (await source.OutputAvailableAsync())
            {
                while (source.TryReceive(out TInput data))
                {
                    //bytesProcessed += data.Length;
                }
            }
            return default;
        }

        public void Result(IEnumerable<int> data) 
            => Console.WriteLine($" Values: [{string.Join(",", data)}]\n");

        private int[] ChangeType(object data) 
            => (int[])Convert.ChangeType(data, typeof(int[]));

        private byte[] ToByteArray(object value)
        {
            int rawsize = Marshal.SizeOf(value);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle =
                GCHandle.Alloc(rawdata,
                GCHandleType.Pinned);
            Marshal.StructureToPtr(value,
                handle.AddrOfPinnedObject(),
                false);
            handle.Free();
            return rawdata;
            // if (maxLength < rawdata.Length) {
            //     byte[] temp = new byte[maxLength];
            //     Array.Copy(rawdata, temp, maxLength);
            //     return temp;
            // } else {
            //     return rawdata;
            // }
        }

        private T FromByteArray<T>(byte[] rawValue)
        {
            GCHandle handle = GCHandle.Alloc(rawValue, GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }
    }
}