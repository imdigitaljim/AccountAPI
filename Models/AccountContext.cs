using System.Data.Entity;

namespace AccountManager.Models
{
    public partial class AccountContext : DbContext
    {
        public AccountContext()
            : base("name=AccountContext") { }

        public virtual DbSet<Character> Characters { get; set; }
        public virtual DbSet<WarcraftAcct> WarcraftAccts { get; set; }
        public virtual DbSet<CharSelect> CharSelections { get; set; }



    }
}
