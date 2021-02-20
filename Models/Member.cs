using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore.Models
{
    //todo 1. Importuojamo failo headeriai buvo pakoreguoti:
    //X,Y,object_id => x,y,OID

    [Table("TestMembers")]
    public class Member
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OID { get; set; }

        //[StringLength(20)]
        public string x { get; set; }

        //[StringLength(20)]
        public string y { get; set; }

        public string case_code { get; set; }

        //[DataType(DataType.DateTime), Required]
        public DateTime confirmation_date { get; set; }

        //[Required, StringLength(3)]
        public string municipality_code { get; set; }

        //[Required, StringLength(25)]
        public string municipality_name { get; set; }

        //[Required, StringLength(25)]
        public string age_bracket { get; set; }

        //[Required, StringLength(11)]
        public string gender { get; set; }

        public Member()
        {
        }
    }
}
