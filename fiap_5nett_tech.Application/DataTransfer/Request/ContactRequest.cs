using System.ComponentModel.DataAnnotations;

namespace fiap_5nett_tech.Application.DataTransfer.Request
{
    public class ContactRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(80, ErrorMessage = "O Nome deve contar até 80 caracteres")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Formato de telefone inválido.")]
        public string PhoneNumber { get; set; }       
        
        public int Ddd { get; set; }

    }
}
