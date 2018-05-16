using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    class PartExpression
    {
        public string Value0 = null;
        public string Value1 = null;
        public string ComparativeOperator = null;
        public string LogicalOperator = null;
        public bool Evaluated = false;

        public void SetValue(string value)
        {
            if (Value0 == null) Value0 = value;
            else Value1 = value;
        }

        public void EvaluateExpression()
        {
            if (Value0 != null && ComparativeOperator == null)
            {
                if (TypeCast.IsBool(Value0)) Evaluated = TypeCast.ToBool(Value0);
            }
            else if (Value0 != null && ComparativeOperator != null && Value1 != null)
            {
                if (Value0.Contains("\"") || Value1.Contains("\""))
                {
                    Value0 = Value0.Replace("\"", "");
                    Value1 = Value1.Replace("\"", "");
                }

                if (ComparativeOperator == ">" || ComparativeOperator == "<" || ComparativeOperator == ">=" || ComparativeOperator == "<=")
                {
                    if (ComparativeOperator == ">") Evaluated = TypeCast.ToDouble(Value0) > TypeCast.ToDouble(Value1);
                    else if (ComparativeOperator == "<") Evaluated = TypeCast.ToDouble(Value0) < TypeCast.ToDouble(Value1);
                    else if (ComparativeOperator == ">=") Evaluated = TypeCast.ToDouble(Value0) >= TypeCast.ToDouble(Value1);
                    else if (ComparativeOperator == "<=") Evaluated = TypeCast.ToDouble(Value0) <= TypeCast.ToDouble(Value1);
                }
                else
                {
                    if (ComparativeOperator == "==") Evaluated = Value0 == Value1;
                    else if (ComparativeOperator == "!=") Evaluated = Value0 != Value1;
                }
            }
        }
    }
}
