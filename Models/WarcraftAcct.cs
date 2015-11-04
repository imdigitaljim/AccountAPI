using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace AccountManager.Models
{
    //SQL Data Types that are strings are NVARCHAR(15) to
    //support other languages from other global markets
    //like china

    [DataContract]
    [Table("WarcraftAcct")]
    public partial class WarcraftAcct
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Order = 1)]
        public int account_id { get; set; }

        [DataMember(Order = 2)]
        [Required]
        public string account_name { get; set; }

        [DataMember(Order = 3)]
        public string link = "";

        [DataMember(Order = 4)]
        public bool? active { get; set; }

        //checks for unwanted characters from account name, SQL injection attacks

    }

  public class AccountModelValidation
  {
      private Regex valid_account_entry = new Regex("^[a-zA-Z0-9_]*$");
      private const int MIN_ACCT_LENGTH = 4;
      private const int MAX_STR_LENGTH = 12;
      public bool AccountEntryIsValid(string acct)
      {
          return valid_account_entry.IsMatch(acct)
              && acct.Length >= MIN_ACCT_LENGTH
                 && acct.Length <= MAX_STR_LENGTH;
      }
  }
    public class NewAccount
    {
        public string name { get; set; }
    }
}
