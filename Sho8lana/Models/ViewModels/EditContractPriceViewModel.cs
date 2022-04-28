using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models.ViewModels
{
    public class EditContractPriceViewModel
    {
        public int Id { get; set; }
        [
            DataType(DataType.Currency)
            ,Display(Name ="Contract Price")
            ,Range(5,100000
            ,ErrorMessage ="Enter a value from 5$ to 100k$")
        ]
        public float Price { get; set; }

    }
}
