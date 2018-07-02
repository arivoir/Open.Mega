using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Mega
{
    public class MegaEncryptStream : MegaStream
    {
        public MegaEncryptStream(Stream plainStream)
            : base(plainStream, Crypto.CreateAesKey(), Crypto.CreateAesKey().CopySubArray(8))
        {
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            byte[] input = new byte[count];

            int totalReadBytes = 0;
            int stepReadBytes = 1, stepOffset = 0, stepCount = count;
            while (stepReadBytes > 0 && totalReadBytes < count)
            {
                stepReadBytes = await base.ReadAsync(input, stepOffset, stepCount, cancellationToken);
                totalReadBytes += stepReadBytes;
                stepOffset += stepReadBytes;
                stepCount -= stepReadBytes;
            }
            await Task.Run(() => ProcessChunk(MegaStreamMode.Encrypt, input, 0, buffer, offset, totalReadBytes));
            Position += totalReadBytes;
            return totalReadBytes;
        }
    }

    public class MegaDecryptStream : MegaStream
    {
        private const int CHUNKSIZE = 65536;
        private byte[] expectedMetaMac;

        public MegaDecryptStream(Stream encryptedStream, long streamLength, byte[] fileKey, byte[] iv, byte[] expectedMetaMac)
            : base(encryptedStream, streamLength, fileKey, iv)
        {
            this.expectedMetaMac = expectedMetaMac;
        }

        private byte[] _input = new byte[CHUNKSIZE];
        private byte[] _lastChunk = new byte[CHUNKSIZE];
        private long _nextChunkPosition = 0;

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalReadBytes = 0;
            while (count > 0 && Position < Length)
            {
                if (Position < _nextChunkPosition)
                {
                    var chunkOffset = (int)(Position % CHUNKSIZE);
                    int bytesFromLastChunk = MathEx.Min(CHUNKSIZE - chunkOffset, count, (int)(Length - Position));
                    Array.Copy(_lastChunk, chunkOffset, buffer, offset, bytesFromLastChunk);
                    count -= bytesFromLastChunk;
                    offset += bytesFromLastChunk;
                    totalReadBytes += bytesFromLastChunk;
                    Position += bytesFromLastChunk;
                }

                if (count > 0)
                {
                    int bytesToRead = Math.Min(CHUNKSIZE, (int)(Length - Position));
                    var readBytes = await base.ReadAsync(_input, 0, bytesToRead, cancellationToken);
                    await Task.Run(() =>
                    {
                        ProcessChunk(MegaStreamMode.Decrypt, _input, 0, _lastChunk, 0, readBytes);
                    });
                    _nextChunkPosition += readBytes;
                    var bytesToWrite = Math.Min(count, readBytes);
                    Array.Copy(_lastChunk, 0, buffer, offset, bytesToWrite);
                    count -= bytesToWrite;
                    offset += bytesToWrite;
                    totalReadBytes += bytesToWrite;
                    Position += bytesToWrite;
                }
            }
            return totalReadBytes;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!expectedMetaMac.SequenceEqual(_metaMac))
                throw new Exception("File could not be decrypted");
        }
    }

    public abstract class MegaStream : Stream
    {
        protected Stream _stream;
        protected long _streamLength;
        protected long _position = 0;
        protected readonly byte[] _fileKey;
        protected readonly byte[] _iv;
        protected byte[] _metaMac = new byte[8];

        private readonly long[] _chunksPositions;
        private readonly byte[] _counter = new byte[8];
        private long _currentCounter = 0;
        protected byte[] _currentChunkMac = new byte[16];
        protected byte[] _fileMac = new byte[16];


        //private CryptographicKey _fileCryptoKey;

        public MegaStream(Stream plainStream, long streamLength, byte[] fileKey, byte[] iv)
        {
            _stream = plainStream;
            _streamLength = streamLength;
            _chunksPositions = GetChunksPositions(_streamLength);
            _fileKey = fileKey;
            //_fileCryptoKey = Crypto.CreateCryptographicKey(_fileKey.AsBuffer());
            _iv = iv;
        }

        public MegaStream(Stream plainStream, byte[] fileKey, byte[] iv)
            : this(plainStream, plainStream.Length, fileKey, iv)
        {
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
        private bool _isDisposing = false;
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposing)
            {
                _isDisposing = true;
                // When stream is fully processed, we compute the last chunk
                if (Position == Length)
                {
                    ComputeChunk(ref _fileMac, _currentChunkMac, _fileKey);

                    // Compute Meta MAC
                    for (int i = 0; i < 4; i++)
                    {
                        _metaMac[i] = (byte)(_fileMac[i] ^ _fileMac[i + 4]);
                        _metaMac[i + 4] = (byte)(_fileMac[i + 8] ^ _fileMac[i + 12]);
                    }

                }
                base.Dispose(disposing);
            }
        }
        protected void ProcessChunk(MegaStreamMode mode, byte[] source, int sourceIndex, byte[] destination, int destinationIndex, int count)
        {
            var chunkLength = Math.Min(source.Length - sourceIndex, (int)(_streamLength - _position));
            if (chunkLength > 0)
            {
                for (long pos = _position; pos < _position + chunkLength; pos += 16)
                {
                    int itPos = (int)(pos - _position);
                    // We are on a chunk bondary
                    if (_chunksPositions.Any(cp => cp == pos) || pos == 0)
                    {
                        if (pos != 0)
                        {
                            // Compute the current chunk mac data on each chunk bondary
                            ComputeChunk(ref _fileMac, _currentChunkMac, _fileKey);
                        }

                        // Init chunk mac with Iv values
                        for (int i = 0; i < 8; i++)
                        {
                            _currentChunkMac[i] = _iv[i];
                            _currentChunkMac[i + 8] = _iv[i];
                        }
                    }

                    IncrementCounter(ref _currentCounter, _counter);

                    // Iterate each AES 16 bytes block
                    byte[] output = new byte[16];
                    byte[] input = new byte[16];
                    int inputLength = Math.Min(input.Length, (int)chunkLength - (int)itPos);
                    Array.Copy(source, sourceIndex + itPos, input, 0, inputLength);

                    // Merge Iv and counter
                    byte[] ivCounter = new byte[16];
                    Array.Copy(_iv, ivCounter, 8);
                    Array.Copy(_counter, 0, ivCounter, 8, 8);

                    byte[] encryptedIvCounter = Crypto.EncryptAes(ivCounter, _fileKey);

                    for (int inputPos = 0; inputPos < inputLength; inputPos++)
                    {
                        output[inputPos] = (byte)(encryptedIvCounter[inputPos] ^ input[inputPos]);
                        _currentChunkMac[inputPos] ^= (mode == MegaStreamMode.Encrypt) ? input[inputPos] : output[inputPos];
                    }

                    // Copy to buffer
                    Array.Copy(output, 0, destination, destinationIndex + (int)itPos, inputLength);

                    // Crypt to current chunk mac
                    _currentChunkMac = Crypto.EncryptAes(_currentChunkMac, _fileKey);
                }

            }
        }

        public byte[] FileKey
        {
            get
            {
                return _fileKey;
            }
        }

        public byte[] IV
        {
            get
            {
                return _iv;
            }
        }

        public byte[] MetaMac
        {
            get
            {
                return _metaMac;
            }
        }

        private static void IncrementCounter(ref long currentCounter, byte[] counter)
        {
            byte[] bytes = BitConverter.GetBytes(currentCounter++);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, counter, 8);
        }

        protected static void ComputeChunk(ref byte[] fileMac, byte[] currentChunkMac, byte[] fileKey)
        {
            for (int i = 0; i < 16; i++)
            {
                fileMac[i] ^= currentChunkMac[i];
            }

            fileMac = Crypto.EncryptAes(fileMac, fileKey);
        }

        private static long[] GetChunksPositions(long size)
        {
            var chunks = new List<long>();

            long chunkStartPosition = 0;
            for (long idx = 1; (idx <= 8) && (chunkStartPosition < (size - (idx * 131072))); idx++)
            {
                chunkStartPosition += idx * 131072;
                chunks.Add(chunkStartPosition);
            }

            while ((chunkStartPosition + 1048576) < size)
            {
                chunkStartPosition += 1048576;
                chunks.Add(chunkStartPosition);
            }

            return chunks.ToArray();
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return _streamLength; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _position = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    public enum MegaStreamMode
    {
        Encrypt,
        Decrypt
    }
}
