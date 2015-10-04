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
    class PeekTestStream : PeekableStream
    {

        public MemoryStream ReferenceBytes;
        public MemoryStream PeekBytes;
        public byte[] InputBytes;
        private Action<PeekTestStream> _peek;

        public PeekTestStream(byte[] inputBytes, Stream stream, Action<PeekTestStream> peek)
            : base(stream, true, false)
        {
            _peek = peek;
            InputBytes = inputBytes;
            ReferenceBytes = new MemoryStream();
            PeekBytes = new MemoryStream();

            Init();

            Assert.IsTrue(ReferenceBytes.ToArray().SequenceEqual(PeekBytes.ToArray()));
            Assert.IsTrue(_maxPos == PeekReadMaxPosition);
        }

        protected override void Peek()
        {
            _peek(this);
        }

        private long _pos;
        private long _maxPos;

        public void TSeek(long offset, SeekOrigin so)
        {
            if (so == SeekOrigin.Begin)
                _pos = offset;
            else if (so == SeekOrigin.Current)
                _pos = _pos + offset;
            else
                throw new NotSupportedException();

            long ret = Seek(offset, so);

            Assert.IsTrue(ret == _pos);
            Assert.IsTrue(Position == _pos);

            _maxPos = _pos > _maxPos ? _pos : _maxPos;
        }

        public void TPositionSet(long offset)
        {
            _pos = offset;

            Position = offset;

            Assert.IsTrue(Position == offset);

            _maxPos = _pos > _maxPos ? _pos : _maxPos;
        }

        public void TRead(int count)
        {
            int realCount = (_pos + count) < InputBytes.Length ? count : (InputBytes.Length - (int)_pos);
            byte[] realBuf = new byte[count];
            Buffer.BlockCopy(InputBytes, (int)_pos, realBuf, 0, realCount);
            ReferenceBytes.Write(realBuf, 0, realCount);
            _pos = _pos + realCount;

            byte[] buff = new byte[count];
            int r = Read(buff, 0, count);
            PeekBytes.Write(buff, 0, r);

            Assert.IsTrue(realCount == r);
            Assert.IsTrue(Position == _pos);
            Assert.IsTrue(buff.SequenceEqual(realBuf));

            _maxPos = _pos > _maxPos ? _pos : _maxPos;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ReferenceBytes.Dispose();
            PeekBytes.Dispose();
        }

    }
}
