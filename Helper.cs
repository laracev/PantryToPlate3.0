namespace PantryToPlate
{
    public static class Helper
    {
        public static double ToDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            double.TryParse(text.Replace(',', '.'), out double result);
            return result;
        }
    }
}