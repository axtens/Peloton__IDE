namespace Peloton_IDE.Presentation
{
    class LongestToShortestLengthComparer : IComparer<String>
    {
        public int Compare(string? x, string? y)
        {
            int lengthComparison = x.Length.CompareTo(y.Length);
            if (lengthComparison == 0)
            {
                return x.CompareTo(y) * -1;
            }
            else
            {
                return lengthComparison * -1;
            }
        }
    }
}
