using DedupSharp.Apps.ImageProcessing.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DedupSharp.Apps.ImageProcessing
{
    public class ImageProcessingStages
    {
        //Phase 1
        public static void LoadPipelinedImages(IEnumerable<string> fileNames, string sourceDir, BlockingCollection<Image> original, CancellationTokenSource cts)
        {
            int count = 0;
            int clockOffset = Environment.TickCount;
            var token = cts.Token;
            Image info = null;
            try
            {
                foreach (var fileName in fileNames)
                {
                    if (token.IsCancellationRequested)
                        break;
                    info = ImageOperations.LoadImage(fileName, sourceDir, count, clockOffset);
                    original.Add(info, token);
                    count += 1;
                    info = null;
                }
            }
            catch (Exception e)
            {
                // in case of exception, signal shutdown to other pipeline tasks
                cts.Cancel();
                if (!(e is OperationCanceledException))
                    throw;
            }
            finally
            {
                original.CompleteAdding();
                if (info != null) info.Dispose();
            }
        }

        //Phase 2
        public static void ScalePipelinedImages(BlockingCollection<Image> originalImages, BlockingCollection<Image> thumbnailImages, CancellationTokenSource cts)
        {
            var token = cts.Token;
            Image info = null;
            try
            {
                foreach (var infoTmp in originalImages.GetConsumingEnumerable())
                {
                    info = infoTmp;
                    if (token.IsCancellationRequested)
                        break;
                    ImageOperations.ScaleImage(info);
                    thumbnailImages.Add(info, token);
                    info = null;
                }
            }
            catch (Exception e)
            {
                cts.Cancel();
                if (!(e is OperationCanceledException))
                    throw;
            }
            finally
            {
                thumbnailImages.CompleteAdding();
                if (info != null) info.Dispose();
            }
        }


        //Phase 3
        public static void FilterPipelinedImages(
            BlockingCollection<Image> thumbnailImages,
            BlockingCollection<Image> filteredImages,
            CancellationTokenSource cts)
        {
            Image info = null;
            try
            {
                var token = cts.Token;
                foreach (Image infoTmp in thumbnailImages.GetConsumingEnumerable())
                {
                    info = infoTmp;
                    if (token.IsCancellationRequested)
                        break;
                    ImageOperations.FilterImage(info);
                    filteredImages.Add(info, token);
                    info = null;
                }
            }
            catch (Exception e)
            {
                cts.Cancel();
                if (!(e is OperationCanceledException))
                    throw;
            }
            finally
            {
                filteredImages.CompleteAdding();
                if (info != null) info.Dispose();
            }
        }

        //Phase 4
        public static void DisplayPipelinedImages(IEnumerable<Image> filteredImages,
            Action<Image> displayFn,
            Action<Image> updateStatisticsFn,
            CancellationTokenSource cts)
        {
            int count = 1;
            int duration = 0;
            var token = cts.Token;
            Image info = null;
            try
            {
                foreach (Image infoTmp in filteredImages)
                {
                    info = infoTmp;
                    if (token.IsCancellationRequested)
                        break;

                    int displayStart = Environment.TickCount;
                    updateStatisticsFn(info);
                    ImageOperations.DisplayImage(info, count, displayFn, duration);
                    duration = Environment.TickCount - displayStart;

                    count = count + 1;
                    info = null;
                }
            }
            catch (Exception e)
            {
                cts.Cancel();
                if (!(e is OperationCanceledException))
                    throw;
            }
            finally
            {
                if (info != null) info.Dispose();
            }
        }

        public static void DisposeImagesInQueue(BlockingCollection<Image> queue)
        {
            if (queue != null)
            {
                queue.CompleteAdding();
                foreach (var info in queue)
                {
                    info.Dispose();
                }
            }
        }
    }
}