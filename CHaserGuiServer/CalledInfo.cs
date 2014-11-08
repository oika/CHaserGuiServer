using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public struct CalledInfo
    {
        public MethodKind Method { get; private set; }
        public DirectionKind Direction { get; private set; }

        public static CalledInfo Parse(string sentChars)
        {
            if (sentChars.Length != 2) throw new ArgumentException(sentChars + "を命令に変換できません");

            var mVal = sentChars[0].ToString();
            var dVal = sentChars[1].ToString();

            var info = new CalledInfo();
            info.Method = Enum.GetValues(typeof(MethodKind))
                              .Cast<MethodKind>()
                              .Single(m => m.ToChar() == mVal);

            info.Direction = Enum.GetValues(typeof(DirectionKind))
                                 .Cast<DirectionKind>()
                                 .Single(d => d.ToChar() == dVal);
            return info;
        }
    }
}
