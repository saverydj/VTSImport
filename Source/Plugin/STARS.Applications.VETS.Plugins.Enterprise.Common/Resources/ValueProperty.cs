namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    public class ValueProperty
    {
        public ValueProperty(string unit, double value)
        {
            Unit = unit;
            Value = value;
        }

        public string Unit { get; private set; }

        public double Value { get; private set; }
    }
}
