namespace FinLib
{
  public class PatternInterval
  {
    public double x;
    public double y;

    public PatternInterval()
    {
      x = y = -1;
    }

    public PatternInterval(double startIndex, double endIndex)
    {
      x = startIndex;
      y = endIndex;
    }
  }
}
