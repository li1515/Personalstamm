using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personalstamm
{
public class RecordComparer : IEqualityComparer<JsonRecord>
    {
        public int GetHashCode(JsonRecord co)
        {
            if (co == null)
            {
                return 0;
            }
            return (co.Name+co.Gehalt).GetHashCode();
        }

        public bool Equals(JsonRecord x1, JsonRecord x2)
        {
            if (object.ReferenceEquals(x1, x2))
            {
                return true;
            }
            if (object.ReferenceEquals(x1, null) ||
                object.ReferenceEquals(x2, null))
            {
                return false;
            }
            return (x1.Name + x1.Gehalt) == (x2.Name+
                    x2.Gehalt);
        }
    }
}
