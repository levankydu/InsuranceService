using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace test0000001.Models
{
    [Table(name: ("tbCarInsuredObject"))]
    public class CarInsuredObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime YearsOfManufacture { get; set; }
        
        [StringLength(50)]
        public string? Automaker { get; set; } 
        [StringLength(50)]
        public string? CarBand { get; set; } 
        [StringLength(50)]
        public string? CarType { get; set; } 
                                             
        [StringLength(50)]
        public string? EngineNumber { get; set; }  //Số máy (Engine Number):
        [StringLength(50)]
        public string? FrameNumber { get; set; } //Số khung Frame Number

        [StringLength(50)]
        public string? LicensePlateNumber { get; set; } // Số biển kiểm soát (License Plate Number)

        [StringLength(50)]
        public string? EngineDisplacement { get; set; }//Engine Displacement

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? CityOfCarReg { get; set; }
        public string? FrontImg { get; set; }
        public string? BackImg { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }


    }
}
