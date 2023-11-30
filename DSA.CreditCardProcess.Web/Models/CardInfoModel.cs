using System.ComponentModel.DataAnnotations;

namespace DSA.CreditCardProcess.Web.Models
{
    public class CardInfoModel
    {
        [Required(ErrorMessage = "El número de tarjeta es requerido.")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Número de tarjeta invalido.")]
        public string CardNumber { get; set; } = null!;

        public string CardNumberNoMask { get; set; } = null!;
    }
}
