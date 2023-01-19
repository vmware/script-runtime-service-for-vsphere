using System;
using System.IO;

namespace VMware.ScriptRuntimeService.AdminEngine.K8sClient {
   internal class PodLogReader : Stream, IDisposable {

      private StreamReader _baseStreamReader;

      public PodLogReader(Stream stream) {
         _baseStreamReader = new StreamReader(stream);
      }

      public override void Flush() {
         // Not applicable as we don't support writing
      }

      private byte[] _buffer;
      private int _bufferIndex;

      public override int Read(byte[] buffer, int offset, int count) {
         int result = 0;
         for (int i = 0; i < count; i++) {
            if (EnsureDataAvailable()) {
               buffer[i] = _buffer[_bufferIndex];
               _bufferIndex++;
               result++;
            } else {
               break;
            }
         }

         return result;
      }

      private bool EnsureDataAvailable() {
         bool result = true;

         if (null == _buffer || _bufferIndex >= _buffer.Length) {
            result = RefillBuffer();
         }

         // go through empty lines - should be invoked only once
         while (result && _buffer.Length == 0) {
            result = RefillBuffer();
         }

         return result;
      }

      private bool RefillBuffer() {
         var line = ReadNextLine();

         if (null == line) {
            // End of file
            return false;
         }

         if (_buffer != null) {
            line = Environment.NewLine + line;
         }

         _buffer = _baseStreamReader.CurrentEncoding.GetBytes(line);

         _bufferIndex = 0;

         return true;
      }

      private string ReadNextLine() {
         return _baseStreamReader.ReadLine()?
            .Replace("\\n", System.Environment.NewLine)
            .Replace("\u001b[40m\u001b[37mtrce\u001b[39m\u001b[22m\u001b[49m", "trce")
            .Replace("\u001b[40m\u001b[37mdbug\u001b[39m\u001b[22m\u001b[49m", "dbug")
            .Replace("\u001b[40m\u001b[32minfo\u001b[39m\u001b[22m\u001b[49m", "info")
            .Replace("\u001b[40m\u001b[1m\u001b[33mwarn\u001b[39m\u001b[22m\u001b[49m", "warn")
            .Replace("\u001b[41m\u001b[30mfail\u001b[39m\u001b[22m\u001b[49m", "fail")
            .Replace("\u001b[41m\u001b[1m\u001b[37mcrit\u001b[39m\u001b[22m\u001b[49m", "crit");
      }

      public override long Seek(long offset, SeekOrigin origin) {
         var result = _baseStreamReader.BaseStream.Seek(offset, origin);
         _baseStreamReader.DiscardBufferedData();

         return result;
      }

      public override void SetLength(long value) {
         throw new NotSupportedException();
      }

      public override void Write(byte[] buffer, int offset, int count) {
         throw new NotSupportedException();
      }

      public override bool CanRead => true;
      public override bool CanSeek => _baseStreamReader.BaseStream.CanSeek;
      public override bool CanWrite => false;
      public override long Length => _baseStreamReader.BaseStream.Length;
      public override long Position { get; set; }

      protected override void Dispose(bool disposing) {
         try {
            if (_baseStreamReader != null) {
               _baseStreamReader.Dispose();
            }
         } finally {
            if (_baseStreamReader != null) {
               _baseStreamReader = null;
               _buffer = null;
               _bufferIndex = 0;
               base.Dispose(disposing);
            }
         }
      }
   }
}
