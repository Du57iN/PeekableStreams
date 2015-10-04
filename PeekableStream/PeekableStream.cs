using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Du57iN.PeekableStreams
{
    public abstract class PeekableStream : Stream
    {
        private Stream _baseStream;
        private MemoryStream _buffer;
        private bool _peekMode;
        private long _baseStartPosition;
        private bool _closeBase;


        public PeekableStream(Stream baseStream, bool closeBaseStream, bool autoInit)
        {
            _closeBase = closeBaseStream;
            _baseStream = baseStream;
            if (autoInit)
                Init();
        }

        public long PeekReadMaxPosition { get; private set; }

        protected abstract void Peek();

        protected void Init()
        {
            _baseStartPosition = _baseStream.Position;
            if (_baseStream.CanSeek)
            {
                _peekMode = true;
                Peek();
                _peekMode = false;
                _baseStream.Position = _baseStartPosition;
            }
            else
            {
                _peekMode = true;
                _buffer = new MemoryStream();
                Peek();
                _buffer.Position = 0;
                _peekMode = false;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret;
            if (_baseStream.CanSeek)
                ret = _baseStream.Read(buffer, offset, count);
            else
            {
                if (_peekMode)
                {
                    int r2 = 0;
                    int readed = _buffer.Read(buffer, offset, (_buffer.Position + count) < _buffer.Length ? count : (int)(_buffer.Length - _buffer.Position));
                    if (readed < count)
                    {
                        r2 = _baseStream.Read(buffer, offset + readed, count - readed);
                        _buffer.Write(buffer, offset + readed, r2);

                        if (PeekReadMaxPosition < _buffer.Position)
                            PeekReadMaxPosition = _buffer.Position;
                    }
                    ret = readed + r2;
                }
                else
                {
                    if (_buffer != null && Position < _buffer.Length)
                    {
                        int r2 = 0;
                        int readed = _buffer.Read(buffer, offset, count);
                        if (readed < count)
                        {
                            r2 = _baseStream.Read(buffer, offset + readed, count - readed);
                        }

                        if (Position > _buffer.Length)
                        {
                            //dal neni potreba
                            _buffer.Dispose();
                            _buffer = null;
                        }
                        ret = readed + r2;
                    }
                    else
                        ret = _baseStream.Read(buffer, offset, count);
                }
            }

            if (_peekMode && PeekReadMaxPosition < Position)
                PeekReadMaxPosition = Position;

            return ret;
        }

        public override bool CanSeek
        {
            get { return _baseStream.CanSeek || _peekMode; }
        }

        public override long Position
        {
            get
            {
                if (_baseStream.CanSeek)
                    return _baseStream.Position;
                else
                {
                    if (_peekMode)
                        return _buffer.Position;
                    else
                    {
                        if (_buffer != null && _buffer.Position < _buffer.Length)
                            return _buffer.Position;
                        else
                            return _baseStream.Position;
                    }
                }
            }
            set
            {
                if (_baseStream.CanSeek)
                    _baseStream.Position = value;
                else
                {
                    if (_peekMode)
                    {
                        if (value < _baseStartPosition)
                            throw new Exception("if base stream is NonSeekable, position value cannot be less then base stream starting position");

                        if (value < _buffer.Length)
                            _buffer.Position = value;
                        else
                        {
                            _buffer.Position = _buffer.Length;
                            byte[] buff = new byte[value - _buffer.Length];
                            Read(buff, 0, buff.Length);
                        }
                    }
                    else
                        throw new NotSupportedException();
                }

                if (_peekMode && PeekReadMaxPosition < Position)
                    PeekReadMaxPosition = Position;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long ret;
            if (_baseStream.CanSeek)
                ret = _baseStream.Seek(offset, origin);
            else
            {
                if (_peekMode)
                {
                    if (origin == SeekOrigin.Begin)
                        Position = offset;
                    else if (origin == SeekOrigin.End)
                        throw new NotSupportedException("if base stream is NonSeekable, seek from SeekOrigin.End not supported");
                    else if (origin == SeekOrigin.Current)
                        Position = Position + offset;

                    ret = Position;
                }
                else
                    throw new NotSupportedException();
            }

            if (_peekMode && PeekReadMaxPosition < Position)
                PeekReadMaxPosition = Position;

            return ret;
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
            get { return false; }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }


        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
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
