using Moq;
using fiap_5nett_tech.Application.Service;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.Test.Services;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _mockContactRepository;
    private readonly Mock<IRegionRepository> _mockRegionRepository;
    private readonly ContactService _contactService;
    
    public ContactServiceTests()
    {
        _mockContactRepository = new Mock<IContactRepository>();
        _mockRegionRepository = new Mock<IRegionRepository>();
        _contactService = new ContactService(_mockContactRepository.Object, _mockRegionRepository.Object);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Create_ShouldReturnContactResponse_WhenContactIsCreatedSuccessfully()
    {
        
        var request = new ContactRequest
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            Ddd = 11
        };

        var region = new Region { Ddd = 11, Name = "São Paulo" };
        _mockRegionRepository.Setup(r => r.GetOne(request.Ddd)).Returns(region);
        _mockContactRepository.Setup(c => c.GetOne(request.Ddd, request.PhoneNumber)).Returns((Contact)null);

        
        var response = _contactService.Create(request);

        
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Contato criado com sucesso!", response.Message);
        Assert.NotNull(response.Data);
        _mockContactRepository.Verify(c => c.Create(It.IsAny<Contact>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Create_ShouldReturnBadRequest_WhenRegionNotFound()
    {
        
        var request = new ContactRequest
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            Ddd = 11
        };

        _mockRegionRepository.Setup(r => r.GetOne(request.Ddd)).Returns((Region)null);

        
        var response = _contactService.Create(request);

        var resposta = response.Message.Normalize();
        
        Assert.NotNull(response);
        Assert.Equal(400, response.Code);
        Assert.Equal("Região não encontrada!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Create(It.IsAny<Contact>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Create_ShouldReturnBadRequest_WhenPhoneNumberAlreadyExists()
    {
        
        var request = new ContactRequest
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            Ddd = 11
        };

        var existingContact = new Contact("Jane Doe", "jane.doe@example.com", "123456789", new Region { Ddd = 11 });
        _mockRegionRepository.Setup(r => r.GetOne(request.Ddd)).Returns(new Region { Ddd = 11, Name = "São Paulo" });
        _mockContactRepository.Setup(c => c.GetOne(request.Ddd, request.PhoneNumber)).Returns(existingContact);

        var response = _contactService.Create(request);

        Assert.NotNull(response);
        Assert.Equal(400, response.Code);
        Assert.Equal("Telefone já Cadastrado!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Create(It.IsAny<Contact>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Create_ShouldReturnBadRequest_WhenPhoneNumberIsInvalid()
    {
        
        var request = new ContactRequest
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "12345",
            Ddd = 11
        };

        _mockRegionRepository.Setup(r => r.GetOne(request.Ddd)).Returns(new Region { Ddd = 11, Name = "São Paulo" });

        var response = _contactService.Create(request);
        
        Assert.NotNull(response);
        Assert.Equal(400, response.Code);
        Assert.Equal("Quantidade de caracteres de telefone invalido!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Create(It.IsAny<Contact>()), Times.Never);
    }


    [Fact]
    [Trait("Category", "UnitTest")]
    public void Update_ShouldReturnContactResponse_WhenContactIsUpdatedSuccessfully()
    {
        var existingContact = new Contact("John Doe", "john.doe@example.com", "123456789", new Region { Ddd = 11 });
        var request = new ContactRequest
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            PhoneNumber = "123456789",
            Ddd = 11
        };

        _mockContactRepository.Setup(c => c.GetOne(request.Ddd, request.PhoneNumber)).Returns(existingContact);

        var response = _contactService.Update(request);

        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Contato atualizado com sucesso!", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal("Jane Doe", response.Data.Name);
        Assert.Equal("jane.doe@example.com", response.Data.Email);
        _mockContactRepository.Verify(c => c.Update(existingContact), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Update_ShouldReturnNotFound_WhenContactDoesNotExist()
    {
        
        var request = new ContactRequest
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            PhoneNumber = "123456789",
            Ddd = 11
        };

        _mockContactRepository.Setup(c => c.GetOne(request.Ddd, request.PhoneNumber)).Returns((Contact)null);
        
        var response = _contactService.Update(request);
                
        Assert.NotNull(response);
        Assert.Equal(404, response.Code);
        Assert.Equal("Contato não encontrado!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Update(It.IsAny<Contact>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Update_ShouldNotChangeFields_WhenRequestFieldsAreNullOrEmpty()
    {
        
        var existingContact = new Contact("John Doe", "john.doe@example.com", "123456789", new Region { Ddd = 11 });
        var request = new ContactRequest
        {
            PhoneNumber = "123456789",
            Ddd = 11,
            Name = "",
            Email = ""
        };

        _mockContactRepository.Setup(c => c.GetOne(request.Ddd, request.PhoneNumber)).Returns(existingContact);

        var response = _contactService.Update(request);

        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Contato atualizado com sucesso!", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal("John Doe", response.Data.Name);
        Assert.Equal("john.doe@example.com", response.Data.Email);
        _mockContactRepository.Verify(c => c.Update(existingContact), Times.Once);
    }
    [Fact]
    [Trait("Category", "UnitTest")]
    public void Delete_ShouldReturnContactResponse_WhenContactIsDeletedSuccessfully()
    {
        
        var existingContact = new Contact("John Doe", "john.doe@example.com", "123456789", new Region { Ddd = 11 });

        _mockContactRepository.Setup(c => c.Delete(11, "123456789")).Returns(existingContact);

        var response = _contactService.Delete(11, "123456789");

        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Contato excluído com sucesso!", response.Message);
        Assert.NotNull(response.Data);
        _mockContactRepository.Verify(c => c.Delete(11, "123456789"), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Delete_ShouldReturnNotFound_WhenContactDoesNotExist()
    {
        
        _mockContactRepository.Setup(c => c.Delete(11, "123456789")).Returns((Contact)null);

        
        var response = _contactService.Delete(11, "123456789");

        
        Assert.NotNull(response);
        Assert.Equal(404, response.Code);
        Assert.Equal("Contato não encontrado!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Delete(11, "123456789"), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Delete_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        
        _mockContactRepository.Setup(c => c.Delete(11, "123456789")).Throws(new Exception());

        
        var response = _contactService.Delete(11, "123456789");

        
        Assert.NotNull(response);
        Assert.Equal(500, response.Code);
        Assert.Equal("Não foi possível Deletar o Contato!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.Delete(11, "123456789"), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneById_ShouldReturnContactResponse_WhenContactIsFound()
    {
        var contactId = Guid.NewGuid();
        var existingContact = new Contact("John Doe", "john.doe@example.com", "123456789", new Region { Ddd = 11 });

        _mockContactRepository.Setup(c => c.GetOne(contactId)).Returns(existingContact);

        var response = _contactService.GetOne(contactId);

        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(existingContact, response.Data);
        _mockContactRepository.Verify(c => c.GetOne(contactId), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneById_ShouldReturnNotFound_WhenContactDoesNotExist()
    {
        
        var contactId = Guid.NewGuid();

        _mockContactRepository.Setup(c => c.GetOne(contactId)).Returns((Contact)null);

        
        var response = _contactService.GetOne(contactId);

        
        Assert.NotNull(response);
        Assert.Equal(404, response.Code);
        Assert.Equal("Contato não encontrado!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.GetOne(contactId), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneById_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        var contactId = Guid.NewGuid();

        _mockContactRepository.Setup(c => c.GetOne(contactId)).Throws(new Exception());

        var response = _contactService.GetOne(contactId);

        Assert.NotNull(response);
        Assert.Equal(500, response.Code);
        Assert.Equal("Não foi possível recuperar o Contato!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.GetOne(contactId), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneByDddAndPhoneNumber_ShouldReturnContactResponse_WhenContactIsFound()
    {
        var ddd = 11;
        var phoneNumber = "123456789";
        var existingContact = new Contact("John Doe", "john.doe@example.com", phoneNumber, new Region { Ddd = ddd });

        _mockContactRepository.Setup(c => c.GetOne(ddd, phoneNumber)).Returns(existingContact);
        
        var response = _contactService.GetOne(ddd, phoneNumber);
        
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(existingContact, response.Data);
        _mockContactRepository.Verify(c => c.GetOne(ddd, phoneNumber), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneByDddAndPhoneNumber_ShouldReturnNotFound_WhenContactDoesNotExist()
    {
        var ddd = 11;
        var phoneNumber = "123456789";

        _mockContactRepository.Setup(c => c.GetOne(ddd, phoneNumber)).Returns((Contact)null);
        
        var response = _contactService.GetOne(ddd, phoneNumber);
        
        Assert.NotNull(response);
        Assert.Equal(404, response.Code);
        Assert.Equal("Contato não encontrado!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.GetOne(ddd, phoneNumber), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetOneByDddAndPhoneNumber_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        var ddd = 11;
        var phoneNumber = "123456789";

        _mockContactRepository.Setup(c => c.GetOne(ddd, phoneNumber)).Throws(new Exception());
        
        var response = _contactService.GetOne(ddd, phoneNumber);
        
        Assert.NotNull(response);
        Assert.Equal(500, response.Code);
        Assert.Equal("Não foi possível recuperar o Contato!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.GetOne(ddd, phoneNumber), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetAll_ShouldReturnPagedContactResponse_WhenContactsAreFound()
    {
        
        var request = new GetAllContactRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "John",
            Email = "john.doe@example.com",
            Ddd = 11,
            PhoneNumber = "123456789"
        };

        var contacts = new List<Contact>
        {
            new Contact("John Doe", "john.doe@example.com", "123456789", new Region { Ddd = 11 }),
            new Contact("Jane Doe", "jane.doe@example.com", "987654321", new Region { Ddd = 11 })
        };

        _mockContactRepository.Setup(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber))
            .Returns(contacts.AsQueryable());
        
        var response = _contactService.GetAll(request);

        
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(contacts.Count, response.Data.Count);
        Assert.Equal(request.PageSize, response.PageSize);
        Assert.Equal(request.PageNumber, response.CurrentPage);
        _mockContactRepository.Verify(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetAll_ShouldReturnEmptyPagedContactResponse_WhenNoContactsAreFound()
    {
        var request = new GetAllContactRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "NonExistent",
            Email = "nonexistent@example.com",
            Ddd = 11,
            PhoneNumber = "000000000"
        };

        _mockContactRepository.Setup(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber))
            .Returns(new List<Contact>().AsQueryable());
        
        var response = _contactService.GetAll(request);
        
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.TotalCount);
        _mockContactRepository.Verify(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber), Times.Once);
    }
    
    [Fact]
    [Trait("Category", "UnitTest")]
    public void GetAll_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        var request = new GetAllContactRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "John",
            Email = "john.doe@example.com",
            Ddd = 11,
            PhoneNumber = "123456789"
        };

        _mockContactRepository.Setup(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber))
            .Throws(new Exception());
        
        var response = _contactService.GetAll(request);

        
        Assert.NotNull(response);
        Assert.Equal(500, response.Code);
        Assert.Equal("Não foi possível consultar os Contatos!", response.Message);
        Assert.Null(response.Data);
        _mockContactRepository.Verify(c => c.GetAll(
            request.Name,
            request.Email,
            request.Ddd,
            request.PhoneNumber), Times.Once);
    }
}