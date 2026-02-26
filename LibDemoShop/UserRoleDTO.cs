using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class UserRoleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
    }

    public class CreateUserRoleDTO
    {
        public string Title { get; set; } = null!;
    }
}
