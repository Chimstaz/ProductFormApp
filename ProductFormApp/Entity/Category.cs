using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductFormApp.Entity
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(90)]
        [Index(IsUnique = true)]
        public String Name { get; set; }
        public String Description { get; set; }
        List<Product> Products { get; set; }

    }
}
