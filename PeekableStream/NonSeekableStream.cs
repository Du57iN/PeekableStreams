using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Du57iN.PeekableStreams
{
    public class NonSeekableStream : Stream
    {
        private Stream _baseStream;
        private bool _closeBase;

        public NonSeekableStream(Stream baseStream) : this(baseStream, false)
        {
        }

        public NonSeekableStream(Stream baseStream, bool closeBaseStream)
        {
            _baseStream = baseStream;
            _closeBase = closeBaseStream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Position
        {
            get { return _baseStream.Position; }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        #region

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }


        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }


        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (_closeBase)
            {
                _baseStream.Dispose();
                _baseStream = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }

}
