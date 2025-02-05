using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.Test.Entities
{
    public class CreateContact
    {
        [Fact]
        [Trait("Category", "UnitTest")]
        public void CriarContatoComSucesso()
        {
            var name = "Teste";
            var email = "teste@teste.com";
            var phone = "999999999";
            var region = new Region(51, "Rio Grande do Sul");

            var contato = new Contact(name, email, phone, region);

            Assert.Equal(contato.Name, name);
            Assert.Equal(contato.Email, email);
            Assert.Equal(contato.Phone, phone);
            Assert.Equal(contato.Region.Ddd, region.Ddd);
            Assert.Equal(contato.Region.Name, region.Name);
        }        
    }
}