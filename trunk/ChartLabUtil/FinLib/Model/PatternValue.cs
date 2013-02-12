using System.Collections.Generic;

namespace FinLib
{
  public class PatternValue
  {
    public PatternInterval Interval { get; private set; }
    public double Ranking { get; private set; }
    public List<PatternBound> Bound { get; private set; }

    public PatternValue(PatternInterval point, double ranking, IEnumerable<PatternBound> bound)
    {
      Interval = point;
      Ranking = ranking;
      Bound = new List<PatternBound>(bound);
    }
  }
}
