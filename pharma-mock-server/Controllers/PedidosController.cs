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
                    var itensAux = this.GetItens(i);

                    var o = new
                    {
                        number = i,
                        date = DateTime.Now,
                        customer = $"Cliente MMMMMM MMMMMMM MMMMMM MMMMM {i}",
                        status = "Pendente",
                        itens = itensAux,
                        margin = this.GetMarginOrder(itensAux),
                        totalOrder = this.GetTotalOrder(itensAux),
                        qtdeItens = itensAux.Count
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

        [HttpPost("calcularValoresPedido")]
        public dynamic calcularValoresPedido([FromBody] dynamic pedidoJson)
        {
            pedidoJson.margin = this.GetMarginOrder(pedidoJson.itens);
            pedidoJson.totalOrder = this.GetTotalOrder(pedidoJson.itens);
            pedidoJson.qtdeItens = pedidoJson.itens.Count;

            return pedidoJson;
        }

        [HttpPost("calcularValoresItem")]
        public dynamic calcularValoresItem([FromBody] dynamic itemJson)
        {
            itemJson.totalItem = itemJson.quantidade * itemJson.salesPrice;
            itemJson.totalCost = itemJson.productUnitCost * itemJson.quantidade;
            itemJson.tax = 0.015 * (double) itemJson.totalItem;
            itemJson.margin = itemJson.totalItem == 0 ? 0 : (itemJson.totalItem - (itemJson.tax + itemJson.totalCost)) / itemJson.totalItem * 100;

            return itemJson;
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
                double qtde = pedido + c;
                double sales = 10 * retorno.Count / 0.2;
                double total = qtde * sales;
                double unitcost = 5.00 / c;
                double totalcost = unitcost * qtde;
                double tx = 0.015 * total;
                double margin = total == 0 ? 0 : (total - (tx + totalcost)) / total;

                var item = new
                {
                    id = Convert.ToString(c),
                    productName = $"Produto XPTO {c} - {pedido}",
                    quantidade = qtde,
                    salesPrice = sales,
                    totalItem = total,
                    productUnitCost = unitcost,
                    totalCost = totalcost,
                    tax = tx,
                    margin = margin
                };
                retorno.Add(item);
            }

            return retorno;
        }

        private double GetMarginOrder(dynamic itens) {
            double marginSum = 0;
            double totalItems = 0;
            try {

                foreach(dynamic i in itens)
                {
                    marginSum = marginSum + i.margin;
                    totalItems = totalItems + i.quantidade;
                }

                return totalItems == 0 ? 0 : marginSum / totalItems;

            }catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                return 0;
            }
        }

        private double GetTotalOrder(dynamic itens) {
            double total = 0;
            foreach (dynamic i in itens)
            {
                total = total + (double) i.totalItem;
            }
            return total;
        }

    }
}