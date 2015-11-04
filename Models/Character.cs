using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace AccountManager.Models
{
    [DataContract]
    public partial class Character
    {

        public int account_id { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Order = 1)]
        public int character_id { get; set; }


        [DataMember(Order = 2)]
        [Required]
        public string name { get; set; }


        [DataMember(Order = 3)]
        [Required]
        public string race { get; set; }

        [Column("class")]
        [DataMember(Order = 4)]
        [Required]
        public string @class { get; set; }

        [DataMember(Order = 5)]
        [Required]
        public string faction { get; set; }

        [DataMember(Order = 6)]
        public int level { get; set; }

        public bool? active { get; set; }

    }

    public class CharacterModelValidation
    {
        private const int MAX_STR_LENGTH = 12;
        private const int MIN_ENTRY_LENGTH = 2;
        //check for alpha characters only
        private Regex valid_char_entry = new Regex("^[a-zA-Z]*$");

		


        //if wanting to check for international characters like 
        //chinese characters eg 聘请我当实习生 or latin characters ñÑ for example
        //then I could make a customized regex using more specific ranges
        //than a seemingly full spectrum like this  ^[a-zA-Z0-9\u0000-\uFFFF\x00-\xFF]*$ 
        //also i do not speak chinese so i have no idea what that says =)

        //additional concerns are offensive words which can be scanned out comparing substrings 

        //as a design decision, i chose to only currently handle alphanumeric or alpha depending on what or
        //if we chose to handle the unicode for a region or all regions this can be customized easily
		
		
        //check for alpha characters and space
        private Regex valid_string_entry = new Regex("^[a-zA-Z ]*$");
        private string FirstToUpper(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        public void UpdateCasing(Character c)
        {
            c.name = FirstToUpper(c.name.ToLower());
            c.faction = FirstToUpper(c.faction.ToLower());
            c.@class = UpdateTitleCase(c.@class);
            c.race = UpdateTitleCase(c.race);
        }

        private string UpdateTitleCase(string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        private bool StringIsValid(string[] s, int min, int max)
        {
            foreach (string i in s)
            {
                if (i.Length > max || i.Length < min
                  || !valid_string_entry.IsMatch(i))
                {
                    return false;
                }
            }
            return true;
        }
        //checks if JSON mapping from request body is set up correctly
        //and is of minimum name length
        public bool CharEntryIsValid(Character ch)
        {
            return valid_char_entry.IsMatch(ch.name)
                && StringIsValid(new string[]
          {
              ch.name,
            ch.faction, 
            ch.race,
            ch.@class
          }, MIN_ENTRY_LENGTH, MAX_STR_LENGTH);
        }



    }
}

