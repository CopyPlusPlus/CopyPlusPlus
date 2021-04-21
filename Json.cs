public class Rootobject
{
    public string From { get; set; }
    public string To { get; set; }
    public TransResult[] TransResult { get; set; }
}

public class TransResult
{
    public string Src { get; set; }
    public string Dst { get; set; }
}