using Microsoft.VisualStudio.TestTools.UnitTesting;
using Du57iN.PeekableStreams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Du57iN.PeekableStreams.Tests
{
    [TestClass]
    public class PeekableStreamTests
    {

        [TestInitialize]
        public void Init()
        {
            InputBytes = Enumerable.Range(0, 200).Select(x=>(byte)x).ToArray();
        }

        protected byte[] InputBytes;


        [TestMethod]
        public void TestPeekBasicWithNonSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(10);
                x.TSeek(20, SeekOrigin.Begin);
                x.TRead(10);
                x.TSeek(10, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(100);
                x.TRead(10);
                x.TRead(10);
                x.TSeek(-30, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(50);
                x.TRead(10);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new NonSeekableStream(new MemoryStream(InputBytes, false), true), peek))
            {
                using (var ms = new MemoryStream())
                {
                    inputStream.CopyTo(ms);
                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

        [TestMethod]
        public void TestPeekBasicWithSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(10);
                x.TSeek(20, SeekOrigin.Begin);
                x.TRead(10);
                x.TSeek(10, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(100);
                x.TRead(10);
                x.TRead(10);
                x.TSeek(-30, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(50);
                x.TRead(10);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new MemoryStream(InputBytes, false), peek))
            {
                using (var ms = new MemoryStream())
                {
                    inputStream.CopyTo(ms);
                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

        [TestMethod]
        public void TestPeekToEndOfStreamWithNonSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(10);
                x.TSeek(InputBytes.Length - 50, SeekOrigin.Begin);
                x.TRead(200);
                x.TSeek(-100, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(10);
                x.TRead(10);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new NonSeekableStream(new MemoryStream(InputBytes, false), true), peek))
            {
                using (var ms = new MemoryStream())
                {
                    inputStream.CopyTo(ms);
                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

        [TestMethod]
        public void TestPeekToEndOfStreamWithSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(10);
                x.TSeek(InputBytes.Length - 50, SeekOrigin.Begin);
                x.TRead(200);
                x.TSeek(-100, SeekOrigin.Current);
                x.TRead(10);
                x.TPositionSet(10);
                x.TRead(10);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new MemoryStream(InputBytes, false), peek))
            {
                using (var ms = new MemoryStream())
                {
                    inputStream.CopyTo(ms);
                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

        [TestMethod]
        public void TestReadOverBufferEdgeStreamWithNonSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(100);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new NonSeekableStream(new MemoryStream(InputBytes, false), true), peek))
            {
                using (var ms = new MemoryStream())
                {

                    byte[] buf = new byte[100];

                    inputStream.Read(buf, 0, 90);
                    Assert.IsTrue(inputStream.Position == 90);
                    ms.Write(buf, 0, 90);
                    inputStream.Read(buf, 0, 50);
                    Assert.IsTrue(inputStream.Position == 140);
                    ms.Write(buf, 0, 50);
                    int r = inputStream.Read(buf, 0, 100);
                    Assert.IsTrue(r == 60);
                    Assert.IsTrue(inputStream.Position == 200);
                    ms.Write(buf, 0, 60);

                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

        [TestMethod]
        public void TestReadToBufferEdgeStreamWithNonSeekableBaseStream()
        {
            Action<PeekTestStream> peek = x =>
            {
                x.TRead(100);
            };

            using (Stream inputStream = new PeekTestStream(InputBytes, new NonSeekableStream(new MemoryStream(InputBytes, false), true), peek))
            {
                using (var ms = new MemoryStream())
                {

                    byte[] buf = new byte[100];

                    inputStream.Read(buf, 0, 90);
                    Assert.IsTrue(inputStream.Position == 90);
                    ms.Write(buf, 0, 90);
                    inputStream.Read(buf, 0, 10);
                    Assert.IsTrue(inputStream.Position == 100);
                    ms.Write(buf, 0, 10);
                    int r = inputStream.Read(buf, 0, 100);
                    Assert.IsTrue(r == 100);
                    Assert.IsTrue(inputStream.Position == 200);
                    ms.Write(buf, 0, 100);

                    Assert.IsTrue(InputBytes.SequenceEqual(ms.ToArray()));
                }
            }
        }

    }
}
