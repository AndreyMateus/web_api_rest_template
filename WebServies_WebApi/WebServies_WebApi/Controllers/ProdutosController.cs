using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServies_WebApi.Database;
using WebServies_WebApi.Models;

namespace WebServies_WebApi.Controllers;

// o V1 é Versionamento de API REST, isso permite que você tenha DIFERENTE APIS com DIFERENTES CONTROLLERS contendo CÓDIGO DIFERENTES.
[Route("api/v1/[controller]")] // macete para sempre pegar o nome do controller
[ApiController] // faz com que a classe seja tratada como um controller de api por de baixo dos panos, como por exemplo os códigos de erro ou status code retornados.
public class ProdutosController : ControllerBase
{
    private readonly ApplicationDBContext _applicationDbContext;

    public ProdutosController(ApplicationDBContext applicationDbContext)
    {
        this._applicationDbContext = applicationDbContext;
    }


    // [HttpGet] // que só quero acessar através do GET
    // public IActionResult PegarProdutos()
    // {
    //     // retornando um JSON
    //     return Ok(new {nome = "Victor Lima", empresa = "School of net"}); 
    // }


    // [HttpGet("{id}")]
    // public IActionResult Get(int id)
    // {
    //     return Ok("Andrey Mateus " + id); 
    // }


    [HttpPost]
    // o [FromBoy] é o corpo da requisitação que está sendo enviada para cá, lembre-se que o c# roda ao lado do servidor.
    public IActionResult Post([FromBody] ProdutoTemp p)
    {
        /* Validação Simples em API REST*/

        /* Validando se o Valor não é incompativél*/
        if (p.Preco <= 0)
        {
            Response.StatusCode = 400;
            return new ObjectResult(new { msg = "O preço do produto não ser menor ou igual a 0" });
        }

        /* Validando se o NOME não está VAZIO */
        if (String.IsNullOrWhiteSpace(p.Nome) || string.IsNullOrEmpty(p.Nome))
        {
            Response.StatusCode = 400;
            return new ObjectResult(new { msg = "O nome não pode ser Vazio" });
        }

        Produto produto = new Produto();
        produto.Nome = p.Nome;
        produto.Preco = p.Preco;
        _applicationDbContext.Produtos.Add(produto);
        _applicationDbContext.SaveChanges();

        Response.StatusCode = 201;
        return new ObjectResult(null);
    }


    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_applicationDbContext.Produtos.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult ListaUm(int id)
    {
        // Caso o produto não seja chado, o First lançará uma Exception, capturaremos ela e retornaremos um BadRequest
        try
        {
            Produto produtoResponse = _applicationDbContext.Produtos.First(p => p.Id == id);

            Response.StatusCode = 201;
            // OBS: o Motivo de Retornar um ObjectResult é porque ele é a  CLASSE BASE de TODOS os STATUS CODE no C#, logo então: ele não possui um STATUS FIXO/DEFINIDO, precisamos definir MANUALMENTE.
            return new ObjectResult(null);
        }
        catch (Exception e)
        {
            // Mudando o StatusCode Retornado para 404, not found.
            Response.StatusCode = 404;

            return new ObjectResult("");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Produto produto = _applicationDbContext.Produtos.Find(id);
        if (produto is not null)
        {
            _applicationDbContext.Remove(produto);
            _applicationDbContext.SaveChanges();
            return Ok();
        }

        // Sim, após o primeiro retorno (que é a instrução "return Ok();" dentro do if), a execução do método será interrompida e nenhum código adicional será executado.
        Response.StatusCode = 404;
        return new ObjectResult(null);
    }

    // Editando os dados
    [HttpPatch]
    public IActionResult Patch([FromBody] Produto p)
    {
        Produto? produto = _applicationDbContext.Produtos.Find(p.Id);
        if (produto is null)
        {
            Response.StatusCode = 400;
            return new ObjectResult(null);
        }

        produto.Nome = p.Nome is not null ? p.Nome : produto.Nome; 
        
        _applicationDbContext.Produtos.Update(produto);
        _applicationDbContext.SaveChanges();
        
        return Ok();
    }

    [HttpPut]
    public IActionResult Put([FromBody] Produto p)
    {
        if (p.Id > 0)
        {
            Produto? produto = _applicationDbContext.Produtos.Find(p.Id);
            if (produto is null)
            {
                Response.StatusCode = 400;
                return new ObjectResult(null);
            }
            if (String.IsNullOrEmpty(p.Nome) || p.Preco < 0)
            {
                Response.StatusCode = 400;
                return new ObjectResult(null);
            }
            
            produto.Id = p.Id;
            produto.Nome = p.Nome;
            produto.Preco = p.Preco;
            _applicationDbContext.Produtos.Update(produto);
            _applicationDbContext.SaveChanges();
        }
        return Ok();
    }
}

public class ProdutoTemp
{
    public string Nome { get; set; }
    public double Preco { get; set; }
}