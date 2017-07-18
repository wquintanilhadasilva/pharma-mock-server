using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace pharma_mock_server.Controllers
{
    [Produces("application/json")]
    [Route("api/pedidos")]
    public class PedidosController : Controller
    {

        private IList<dynamic> listaDePedidos = new List<dynamic>();

        private readonly object syncLock = new object();

        public PedidosController()
        {
            this.carregarPedidos();
        }

        private void carregarPedidos()
        {
            lock (syncLock)
            {
                listaDePedidos = new List<dynamic>();

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
                            totalOrder = this.getTotalOrder(itensAux),
                            cost = this.getCostOrder(itensAux),
                            tax = this.getTaxOrder(itensAux),
                            qtdeItens = itensAux.Count
                        };

                        listaDePedidos.Add(o);

                    }
                }
            }
        }

        // GET api/pedidos
        [HttpGet]
        public IEnumerable<dynamic> Get()
        {
            return listaDePedidos;
        }

        [HttpPost("calcularValoresPedido")]
        public dynamic calcularValoresPedido([FromBody] dynamic pedidoJson)
        {
            pedidoJson.margin = this.GetMarginOrder(pedidoJson.itens);
            pedidoJson.cost = this.getCostOrder(pedidoJson.itens);
            pedidoJson.tax = this.getTaxOrder(pedidoJson.itens);

            pedidoJson.totalOrder = this.getTotalOrder(pedidoJson.itens);
            pedidoJson.qtdeItens = pedidoJson.itens.Count;

            return pedidoJson;
        }

        [HttpPost("calcularValoresItem")]
        public dynamic calcularValoresItem([FromBody] dynamic itemJson)
        {
            itemJson.totalItem = itemJson.quantidade * itemJson.salesPrice;
            itemJson.totalCost = itemJson.productUnitCost * itemJson.quantidade;
            itemJson.tax = 0.015 * (double) itemJson.totalItem;
            itemJson.margin = this.getMargin((double) itemJson.totalItem, (double) itemJson.tax, (double) itemJson.totalCost);

            return itemJson;
        }

        // GET api/pedidos/GetQuantidadePedidos
        [HttpGet("GetQuantidadePedidos")]
        public int GetQuantidadePedidos() {
            return listaDePedidos.Count;
        }

        // GET api/pedidos/getReferenciaAtual
        [HttpGet("getReferenciaAtual")]
        public string getReferenciaAtual()
        {
            return String.Format("{0:MM/yyyy}", DateTime.Now);
        }

        // GET api/pedidos/getFaturamentoGlobal
        [HttpGet("getIndicadoresGlobais")]
        public dynamic getIndicadoresGlobais()
        {

            double totalOrders = 0d;
            double totalCost = 0d;
            double totalTax = 0d;

            var orders = this.Get();

            foreach (var o in orders)
            {
                totalOrders = totalOrders + this.getTotalOrder(o.itens);
                totalCost = totalCost + this.getCostOrder(o.itens);
                totalTax = totalTax + this.getTaxOrder(o.itens);
            }

            dynamic retorno = new
            {
                referencia = this.getReferenciaAtual(),
                faturamentoGlobal = totalOrders,
                margemGlobal = this.getMargin(totalOrders, totalTax, totalCost),
                qtdePedidosGlobal = this.GetQuantidadePedidos()
        };

            return retorno;
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

        //[HttpPost("getMargemGlobal")]
        //public double getMargemGlobal([FromBody] dynamic pedidoJson)
        //{

        //    //var editOrder = JsonConvert.DeserializeObject<dynamic>(pedidoJson);

        //    //exclui o pedido em edição na simulação para adicioná-lo no cálculo depois....
        //    var pedidos = this.Get().Where(p => (int) p.number != (int) pedidoJson.number);

        //    var sumCost = (double) pedidos.Sum(p => (double) p.cost);
        //    var sumTax = (double) pedidos.Sum(p => (double) p.tax);
        //    var sumTotal = (double) pedidos.Sum(p => (double) p.totalOrder);

        //    // Adiciona os dados do pedido em edição
        //    sumCost += this.getCostOrder(pedidoJson.itens);
        //    sumTax += this.getTaxOrder(pedidoJson.itens);
        //    sumTotal += this.getTotalOrder(pedidoJson.itens);

        //    // Calcula a margem global
        //    var margin = this.getMargin(sumTotal, sumTax, sumCost);
            
        //    return margin;
        //}

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
                double margin = this.getMargin(total, tx, totalcost);

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

        private double GetMarginOrder(dynamic itens)
        {
            double totalItems = 0;
            double totalCost = 0;
            double totalTx = 0;
            try
            {

                foreach (dynamic i in itens)
                {
                    totalCost = totalCost + (double)i.totalCost;
                    totalItems = totalItems + (double)i.totalItem;
                    totalTx = totalTx + (double)i.tax;
                }

                return this.getMargin(totalItems, totalTx, totalCost);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return 0d;
            }
        }

        private double getTotalOrder(dynamic itens) {
            double total = 0;
            foreach (dynamic i in itens)
            {
                total = total + (double) i.totalItem;
            }
            return total;
        }

        private double getCostOrder(dynamic itens)
        {
            double total = 0;
            foreach (dynamic i in itens)
            {
                total = total + (double)i.totalCost;
            }
            return total;
        }

        private double getTaxOrder(dynamic itens)
        {
            double total = 0;
            foreach (dynamic i in itens)
            {
                total = total + (double)i.tax;
            }
            return total;
        }
               
        private double getMargin(double totalValue, double taxTotalValue, double costTotalValue)
        {
            return totalValue == 0 ? 0 : (totalValue - (taxTotalValue + costTotalValue)) / totalValue * 100;
        }


        //private double getMargemGlobal()
        //{
        //    var pedidos = this.Get();
        //    double retorno = 0d;

        //    foreach(var o in pedidos)
        //    {
        //        retorno = retorno + (double) o.margin;
        //    }
        //    return retorno;
        //}

    }
}