using System.IO;
using System.Text;
using NUnit.Framework;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;

namespace VMware.ScriptRuntimeService.AdminEngine.Tests {
   public class PodLogReaderTests {
      [SetUp]
      public void Setup() {
      }

      [Test]
      public void Test1() {
         var correctStr = @"
some 
correct text
info: info
trce: trce
fail: fail
dbug: dbug
warn: warn
crit: crit
";
         var stream = new MemoryStream(Encoding.UTF8.GetBytes(correctStr));
         stream.Seek(0, SeekOrigin.Begin);

         var reader = new PodLogReader(stream);

         Assert.AreEqual(correctStr.Trim(), ReadAsString(reader).Trim());
      }

      [Test]
      public void Test2() {
         var input = "\u001b[40m\u001b[37mtrce\u001b[39m\u001b[22m\u001b[49m: some trace\\n\u001b[40m\u001b[37mdbug\u001b[39m\u001b[22m\u001b[49m: some debug\\n\u001b[40m\u001b[32minfo\u001b[39m\u001b[22m\u001b[49m: some info\\n\u001b[40m\u001b[1m\u001b[33mwarn\u001b[39m\u001b[22m\u001b[49m: some warning\\n\u001b[41m\u001b[30mfail\u001b[39m\u001b[22m\u001b[49m: some fail\\n\u001b[41m\u001b[1m\u001b[37mcrit\u001b[39m\u001b[22m\u001b[49m: some critical\\n";
         var output = @"trce: some trace
dbug: some debug
info: some info
warn: some warning
fail: some fail
crit: some critical

";
         var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
         stream.Seek(0, SeekOrigin.Begin);

         var reader = new PodLogReader(stream);

         Assert.AreEqual(output.Trim(), ReadAsString(reader).Trim());
      }

      private static string ReadAsString(PodLogReader stream) {
         using (MemoryStream memStream = new MemoryStream()) {
            int count = 8192;
            byte[] array = new byte[count];

            while (count == array.Length) {
               count = stream.Read(array, 0, array.Length);
               memStream.Write(array, 0, count);
               memStream.Flush();
            }

            return Encoding.UTF8.GetString(memStream.ToArray());
         }
      }
   }
}
