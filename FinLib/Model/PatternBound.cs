namespace FinLib
{
  public class PatternBound
  {
    public double Upper;
    public double Lower;

    public PatternBound()
    {
      Upper = Lower = 0.0;  
    }

    public PatternBound(double upper, double lower)
    {
      Upper = upper;
      Lower = lower;
    }
  }
}
