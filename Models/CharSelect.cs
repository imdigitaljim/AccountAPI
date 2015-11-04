using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountManager.Models
{

    [Table("CharSelect")]
    public partial class CharSelect
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int type_id { get; set; }

        [Required]
        public string faction { get; set; }

        [Required]
        public string race { get; set; }

        [Column("class")]
        [Required]
        public string @class { get; set; }

        public int startlvl { get; set; }

        public int maxlvl { get; set; }

        public bool enabled { get; set; }
    }
}
