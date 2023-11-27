using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MailboxApi.Data.Entities
{
    public interface IEntity
    {
        public int Id { get; }
    }

    public class Entity : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }
    }
}
