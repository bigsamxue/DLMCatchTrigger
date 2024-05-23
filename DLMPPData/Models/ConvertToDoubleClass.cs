using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMPPData.Models {
    public class ConvertToDoubleClass {
        public string ConvertToDouble(string data) {
            double data_double;
            bool data_bool = double.TryParse(data, out data_double);
            if (data_bool) {
                return data_double.ToString();
            }
            else {
                return data;
            }
        }
    }
}
