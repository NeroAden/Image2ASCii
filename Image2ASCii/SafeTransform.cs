using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2ASCii
{
    class SafeTransform
    {
        public static int SafeStringToInt(String Values,int Default)
        {
            try
            {
                int backValues = int.Parse(Values);
                return backValues;
            }
            catch (Exception)
            {
                return Default;
            }
        }
    }
}
