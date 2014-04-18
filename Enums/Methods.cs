using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendSpace.Enums
{
    public struct Methods
    {
        private int value;
        private string name;

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        private Methods(int value, string name)
        {
            this.name = name;
            this.value = value;
        }

        public static readonly Methods ANONYMOUS_UPLOAD_GET_INFO = new Methods(0, "anonymous.uploadGetInfo");

        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.name;
        }
    }
}
