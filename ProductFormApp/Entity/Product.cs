using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductFormApp.Entity
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [StringLength(90)]
        [Index(IsUnique = true)]
        public String Name { get; set; }

        [Required]
        public int UnitsInStock { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        public decimal UnitPrice { get; set; }
    }
}
