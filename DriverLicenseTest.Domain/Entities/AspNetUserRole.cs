using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverLicenseTest.Domain.Entities
{
    public class AspNetUserRole
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        public virtual AspNetUser User { get; set; } = null!;
        public virtual AspNetRole Role { get; set; } = null!;
    }
}
