using System;
using System.Collections.Generic;
using System.Web;

namespace dn_lib.db
{
    public class parameter
    {
        public enum typeParameter { none, integer, text, real };

        typeParameter _type = typeParameter.none; string _name = ""; object _val = null;

        public parameter(string name) { _name = name; }
        public parameter(string name, object value) { _name = name; _val = value; }
        public parameter(string name, int int_value) { _name = name; int_val = int_value; }
        public parameter(string name, string text_value) { _name = name; txt_val = text_value; }
        public parameter(string name, double real_value) { _name = name; real_val = real_value; }

        public string name { get { return _name; } set { _name = value; } }
        public object val { get { return _val; } }
        public int int_val { get { return (int)_val; } set { _type = typeParameter.integer; _val = value; } }
        public string txt_val { get { return (string)_val; } set { _type = typeParameter.text; _val = value; } }
        public double real_val { get { return (double)_val; } set { _type = typeParameter.real; _val = value; } }
        public bool is_null { get { return _type == typeParameter.none; } }
        public typeParameter type { get { return _type; } }
    }
}