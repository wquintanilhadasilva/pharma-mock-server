using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;

namespace pharma_mock_server.Controllers
{
    [Produces("application/json")]
    [Route("api/pedidos")]
    public class PedidosController : Controller
    {

        private static IList<dynamic> listaDePedidos = new List<dynamic>();

        public PedidosController()
        {
            if (listaDePedidos == null || listaDePedidos.Count == 0)
            {
                for (var i = 1; i <= 30; i++)
                {
                    var o = new
                    {
                        number = i,
                        date = DateTime.Now,
                        customer = $"Cliente MMMMMM MMMMMMM MMMMMM MMMMM {i}",
                        status = "Pendente",
                        itens = this.GetItens(i)
                    };

                    listaDePedidos.Add(o);

                }
            }
        }

        // GET api/pedidos
        [HttpGet]
        public IEnumerable<dynamic> Get()
        {

            return listaDePedidos.ToArray();

        }

        // GET api/pedidos/GetQuantidadePedidos
        [HttpGet("GetQuantidadePedidos")]
        public int GetQuantidadePedidos() {
            return listaDePedidos.Count;
        }

        // GET api/pedidos/5
        [HttpGet("{id}")]
        public dynamic Get(int id)
        {
            return listaDePedidos.Where(p => p.number == id).FirstOrDefault(); // this.getPedidos().find((p: Order) => p.number === id);
        }

        // POST api/pedidos/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] dynamic value)
        {
            var p = this.Get(id);

            if(p != null)
            {
                p.number = value.number;
                p.date = value.date;
                p.customer = value.customer;
                p.status = value.status;
                p.itens = value.itens;
            }

        }

        private IList<dynamic> GetItens(int pedido)
        {
            var retorno = new List<dynamic>();

            for (var c = 1; c <= 20; c++)
            {
                var item = new
                {
                    id = Convert.ToString(c),
                    productName = $"Produto XPTO {c} - {pedido}",
                    productUnitCost = 5.00 / c,
                    quantidade = pedido + c,
                    salesPrice = 10 * retorno.Count / 0.2,
                    tax = 1.5 * pedido + retorno.Count,
                    totalItem = c
                };
                retorno.Add(item);
            }

            return retorno;
        }
    }
}