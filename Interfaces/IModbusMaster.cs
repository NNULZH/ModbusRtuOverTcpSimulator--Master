using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IModbusMaster
    {
        public void Connect(string host, int port);
        public void WriteByteAsync(byte b);
        public Task WriteBytesAsync(byte[] bytes);
    }
}
