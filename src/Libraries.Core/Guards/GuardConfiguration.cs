using System.Globalization;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Guards
{
    public class GuardConfiguration<T>
    {
        public GuardConfiguration(T theValue, string description)
        {
            TheValue = theValue;
            Description = description;
        }

        public T TheValue { get; }

        public string Description { get; }

        public GuardCondition<T> Is => new GuardCondition<T>(this);

        public string FormattedDescription(bool firstCharacterUpperCase = false)
        {
            var description = Description;
            if (string.IsNullOrWhiteSpace(description)) description = "the value";
            if (!firstCharacterUpperCase) return Description;
            return description
                       .Substring(0, 1)
                       .ToUpper(CultureInfo.InvariantCulture)
                   + description
                       .Substring(1);
        }
    }
}