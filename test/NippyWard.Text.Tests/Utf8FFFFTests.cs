namespace NippyWard.Text.Tests
{
    public class Utf8FFFFTests : BaseUtf8Tests
    {
        protected override string LowerString => new string
        (
            new char[]
            {
                '\u0165',
                '\u1C9E',
                '\u03B5',
                '\uA77D',
                '\u015F',
                '\u0165'
            }
        );

        protected override string UpperString => new string
        (
            new char[]
            {
                '\u0164',
                '\u1C9E',
                '\u0395',
                '\uA77D',
                '\u015E',
                '\u0164'
            }
        );

        protected override string LowerInequalityString => new string
        (
            new char[]
            {
                '\u0165',
                '\u1C9E',
                '\u03B5',
                '\uA77D',
                '\u015F',
                '\u010F'
            }
        );

        protected override string UpperInequalityString => new string
        (
            new char[]
            {
                '\u0164',
                '\u1C9E',
                '\u0395',
                '\uA77D',
                '\u015E',
                '\u010E'
            }
        );
    }
}
