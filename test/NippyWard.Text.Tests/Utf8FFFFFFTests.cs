namespace NippyWard.Text.Tests
{
    public class Utf8FFFFFFTests : BaseUtf8Tests
    {
        protected override string LowerString => new string
        (
            new char[]
            {
                '\uD801', '\uDC3B',
                '\uD801', '\uDC5A',
                '\uD801', '\uDC29',
                '\uD801', '\uDD13',
                '\uD803', '\uDCE4',
                '\uD801', '\uDC3B'
            }
        );

        protected override string UpperString => new string
        (
            new char[]
            {
                '\uD801', '\uDC13',
                '\uD801', '\uDC5A',
                '\uD801', '\uDC01',
                '\uD801', '\uDD13',
                '\uD803', '\uDCA4',
                '\uD801', '\uDC13'
            }
        );

        protected override string LowerInequalityString => new string
        (
            new char[]
            {
                '\uD801', '\uDC3B',
                '\uD801', '\uDC5A',
                '\uD801', '\uDC29',
                '\uD801', '\uDD13',
                '\uD803', '\uDCE4',
                '\uD801', '\uDC3C'
            }
        );

        protected override string UpperInequalityString => new string
        (
            new char[]
            {
                '\uD801', '\uDC13',
                '\uD801', '\uDC5A',
                '\uD801', '\uDC01',
                '\uD801', '\uDD13',
                '\uD803', '\uDCA4',
                '\uD801', '\uDC14'
            }
        );

    }
}
