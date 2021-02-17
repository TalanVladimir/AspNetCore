using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore.Models
{
    [Table("TestMembers")]
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OID { get; set; }

        public string x { get; set; }

        public string y { get; set; }

        public string case_code { get; set; }

        public DateTime confirmation_date { get; set; }

        public string municipality_code { get; set; }

        public string municipality_name { get; set; }

        public string age_bracket { get; set; }

        public string gender { get; set; }

        public Member()
        {
        }
    }
}
