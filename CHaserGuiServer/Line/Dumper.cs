using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oika.Apps.CHaserGuiServer.Line
{
    public class Dumper
    {
        private ILogger logger;

        public Dumper(ILogger logger)
        {
            this.logger = logger;
        }

        public void Dump(byte[] bytes)
        {
            var sb = new StringBuilder(Environment.NewLine);

            var restBytes = bytes as IEnumerable<byte>;
            while (restBytes.Any())
            {
                sb.AppendLine(string.Join(" ", restBytes.Take(16).Select(b => b.ToString("X2"))));
                restBytes = restBytes.Skip(16);
            }

            logger.Info(sb.ToString());
        }
    }
}
