using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Validation
{
    public class NullableRangeAttribute : RangeAttribute
    {
        public NullableRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public override bool IsValid(object? value)
        {
            // Allow null values
            if (value == null)
                return true;

            // Validate non-null values using base RangeAttribute logic
            return base.IsValid(value);
        }
    }
}

