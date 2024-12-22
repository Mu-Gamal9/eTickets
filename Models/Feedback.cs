using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

namespace eTickets.Models
{
    public class Feedback
    {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public string Comments { get; set; }

        
    }
}
