using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests
{
    public class Converter : TextWriter
    {
        ITestOutputHelper _output;
        
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string message)
        {
            try
            {
                _output.WriteLine(message);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message != "There is no currently active test.")
                {
                    throw;
                }
            }
        }

        public override void WriteLine(string format, params object[] args) => _output.WriteLine(format, args);
    }
}