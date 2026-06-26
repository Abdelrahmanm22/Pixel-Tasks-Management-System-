using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Models
{
    public class Corporation : BaseModel
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Code { get; set; }
        public string Notes { get; set; }
    }
}
