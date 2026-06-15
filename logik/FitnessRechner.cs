namespace PantryToPlate.logik
{
    public class FitnessRechner
    {
        public double BerechneKalorien(double metWert, double gewicht, double dauerMinuten)
        {
            double dauerStunden = dauerMinuten / 60;
            return metWert * gewicht * dauerStunden;
        }
    }
}