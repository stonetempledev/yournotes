using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib.db
{
    public class meta_rule
    {
        string _type, _field, _value;

        public meta_rule(string type, string field, string value)
        {
            _type = type;
            _field = field;
            _value = value;
        }

        public string type { get { return _type; } }
        public string field { get { return _field; } }
        public string value { get { return _value; } }
    }
}