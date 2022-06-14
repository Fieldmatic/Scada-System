using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScadaCoreWCF.models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true)]
        [StringLength(100)]
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public User() { }
        public User(string username, string encryptedPassword)
        {
            Username = username;
            EncryptedPassword = encryptedPassword;
        }

    }
}
