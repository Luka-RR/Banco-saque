using Banco_ASP_2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using static Banco_ASP_2.Models.CalculoDoBanco;
using Microsoft.AspNetCore.Http;

namespace Banco_ASP_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpGet]
        public IActionResult VaiParaOutrosValores(Notasmodels nota)
        {
            var saldoBancoString = HttpContext.Session.GetString("SaldoBanco");
            var saldoBanco = saldoBancoString != null ? Convert.ToDecimal(saldoBancoString) : 0;



            HttpContext.Session.SetString("saldoBanco", saldoBanco.ToString() ?? "0");

            HttpContext.Session.SetString("Saldo", nota.Saldo?.ToString() ?? "0");
            HttpContext.Session.SetInt32("Nota10", nota.Nota10 ?? 0);
            HttpContext.Session.SetInt32("Nota20", nota.Nota20 ?? 0);
            HttpContext.Session.SetInt32("Nota50", nota.Nota50 ?? 0);
            HttpContext.Session.SetInt32("Nota100", nota.Nota100 ?? 0);
            HttpContext.Session.SetInt32("Nota200", nota.Nota200 ?? 0);


            var model = new Notasmodels
            {
                Saldo = HttpContext.Session.GetString("Saldo") != null
        ? Convert.ToDecimal(HttpContext.Session.GetString("Saldo"))
        : 0,

                Nota10 = HttpContext.Session.GetInt32("Nota10") ?? 0,
                Nota20 = HttpContext.Session.GetInt32("Nota20") ?? 0,
                Nota50 = HttpContext.Session.GetInt32("Nota50") ?? 0,
                Nota100 = HttpContext.Session.GetInt32("Nota100") ?? 0,
                Nota200 = HttpContext.Session.GetInt32("Nota200") ?? 0,

            };

           

            ViewBag.SaldoBanco = saldoBanco;

            return View("menu/saqueoutrosvalores/outrosValores", model);
        }


        public IActionResult VoltarParaIndex3(decimal Saldoatualizado)
        {
            var saldoBancoString = HttpContext.Session.GetString("SaldoBanco");
            var saldoBanco = saldoBancoString != null ? Convert.ToDecimal(saldoBancoString) : 0;



            var model = new Notasmodels
            {

                Saldo = Saldoatualizado,
                Nota10 = HttpContext.Session.GetInt32("Nota10") ?? 0,
                Nota20 = HttpContext.Session.GetInt32("Nota20") ?? 0,
                Nota50 = HttpContext.Session.GetInt32("Nota50") ?? 0,
                Nota100 = HttpContext.Session.GetInt32("Nota100") ?? 0,
                Nota200 = HttpContext.Session.GetInt32("Nota200") ?? 0,

            };

            ViewBag.SaldoBanco = saldoBanco;

            return View("menu/Index3", model);
        }

        [HttpGet]
        public IActionResult Saque(int valor, Notasmodels nota)
        {
            var saldoBancoString = HttpContext.Session.GetString("SaldoBanco");
            var saldoBanco = saldoBancoString != null ? Convert.ToDecimal(saldoBancoString) : 0;

            /// -------------------- CASO AS NÃO TENHA CELULA NO BANCO-----------------------------------------------
            HttpContext.Session.SetInt32("Nota10", nota.Nota10 ?? 0);
            HttpContext.Session.SetInt32("Nota20", nota.Nota20 ?? 0);
            HttpContext.Session.SetInt32("Nota50", nota.Nota50 ?? 0);
            HttpContext.Session.SetInt32("Nota100", nota.Nota100 ?? 0);
            HttpContext.Session.SetInt32("Nota200", nota.Nota200 ?? 0);

            var notas = new CalculoDoBanco(valor, nota, saldoBanco);
            if (!notas.OperacaoInvalida)
            {
                TempData["Mensagem"] = "NÃO HÁ NOTA DISPONIVEL NO SISTEMA";


                return RedirectToAction("VoltarParaIndex3", new { Saldoatualizado = nota.Saldo });
            }

            ///-------------------------------------------------------------------------------------------------------------



            HttpContext.Session.SetInt32("Nota10", nota.Nota10 ?? 0);
            HttpContext.Session.SetInt32("Nota20", nota.Nota20 ?? 0);
            HttpContext.Session.SetInt32("Nota50", nota.Nota50 ?? 0);
            HttpContext.Session.SetInt32("Nota100", nota.Nota100 ?? 0);
            HttpContext.Session.SetInt32("Nota200", nota.Nota200 ?? 0);

            var model = new SaqueDosBotoes(valor, nota);

            TempData["SaldoAtualizado"] = model.Saldoatualizado;

            HttpContext.Session.SetString("SaldoBanco", notas.saldoBancoAtualizado.ToString());

            return View("menu/saque/saque", model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        // VERIFICA SE OS CAMPOS ESTÃO PREENCHIDOS 

        [HttpPost]
        public IActionResult SalvarNotas(Notasmodels model)
        {
            if (model.Nota10 == null || model.Nota20 == null || model.Nota50 == null ||
                model.Nota100 == null || model.Nota200 == null || model.Saldo == null)
            {
                ViewBag.Mensagem = "Todos os campos são obrigatórios.";
                return View("Index");
            }

            HttpContext.Session.SetString("Saldo", model.Saldo?.ToString() ?? "0");

            var salBanco = new SomaNotas(model);

            HttpContext.Session.SetString("SaldoBanco", salBanco.saldoBanco.ToString());
            ViewBag.SaldoBanco = salBanco.saldoBanco;

            return View("menu/Index3", model);
        }





        public IActionResult OpcaoDeSaque(string othervalue)
        {



            if (string.IsNullOrWhiteSpace(othervalue))
                return RedirectToAction("OutrosValores");

            // converte centavos em reais
            if (!long.TryParse(othervalue, out long centavos))
            {
                ModelState.AddModelError("othervalue", "Formato de valor inválido.");
                return RedirectToAction("OutrosValores");
            }
            decimal valorSaque = centavos / 100m;

            // só sacar múltiplos de 10
            if (valorSaque % 10 != 0)
            {
                ModelState.AddModelError("othervalue", "Só é possível sacar valores múltiplos de R$ 10,00.");
                return RedirectToAction("OutrosValores");
            }

            // carrega notas do TempData (ou Session)
            var nota = new Notasmodels
            {
                Nota10 = HttpContext.Session.GetInt32("Nota10") ?? 0,
                Nota20 = HttpContext.Session.GetInt32("Nota20") ?? 0,
                Nota50 = HttpContext.Session.GetInt32("Nota50") ?? 0,
                Nota100 = HttpContext.Session.GetInt32("Nota100") ?? 0,
                Nota200 = HttpContext.Session.GetInt32("Nota200") ?? 0,
            };

          

            // calcula opções
            var opcoes = CalculadoraDeNotas.CalcularOpcoesDeSaque(valorSaque, nota.Notas);

            HttpContext.Session.SetObjectAsJson("Opcoes", opcoes);

            // debug opções
            for (int i = 0; i < opcoes.Count; i++)
            {
                if (opcoes[i].Count == 0)
                    Console.WriteLine($"[DEBUG] Opção {i + 1} VAZIA");
                else
                    foreach (var kv in opcoes[i])
                        Console.WriteLine($"[DEBUG] Opção {i + 1}: {kv.Value} x R${kv.Key}");
            }

            HttpContext.Session.SetString("ValorSaque", valorSaque.ToString());

            ViewBag.ValorSaque = valorSaque;
            ViewBag.Opcoes = opcoes;
            return View("menu/saqueoutrosvalores/opcaodesaqur", nota);
        }


        [HttpPost]
        public IActionResult OpcaoClicada(int indiceOpcao)
        {
            // Recuperar as opções salvas na sessão
            var opcoes = HttpContext.Session.GetObjectFromJson<List<Dictionary<int, int>>>("Opcoes");
            if (opcoes == null || indiceOpcao < 0 || indiceOpcao >= opcoes.Count)
                return RedirectToAction("Erro");

            var opcaoEscolhida = opcoes[indiceOpcao];

            // Recuperar o estado atual das notas
            var nota = new Notasmodels
            {
                Nota10 = HttpContext.Session.GetInt32("Nota10") ?? 0,
                Nota20 = HttpContext.Session.GetInt32("Nota20") ?? 0,
                Nota50 = HttpContext.Session.GetInt32("Nota50") ?? 0,
                Nota100 = HttpContext.Session.GetInt32("Nota100") ?? 0,
                Nota200 = HttpContext.Session.GetInt32("Nota200") ?? 0,
            };

            // Subtrair as notas de acordo com a opção escolhida
            foreach (var notas in opcaoEscolhida)
            {
                int tipo = notas.Key;
                int qtd = notas.Value;

                switch (tipo)
                {
                    case 10: nota.Nota10 -= qtd; break;
                    case 20: nota.Nota20 -= qtd; break;
                    case 50: nota.Nota50 -= qtd; break;
                    case 100: nota.Nota100 -= qtd; break;
                    case 200: nota.Nota200 -= qtd; break;
                }
            }


            var saldoBancoString = HttpContext.Session.GetString("SaldoBanco");
            var saldoBanco = saldoBancoString != null ? Convert.ToDecimal(saldoBancoString) : 0;


            var valorSaqueString = HttpContext.Session.GetString("ValorSaque");
            int valor = valorSaqueString != null ? Convert.ToInt32(valorSaqueString) : 0;

            decimal novoSaldo = saldoBanco - valor;

            HttpContext.Session.SetString("SaldoBanco", novoSaldo.ToString());


            nota.Saldo = HttpContext.Session.GetString("Saldo") != null
    ? Convert.ToDecimal(HttpContext.Session.GetString("Saldo"))
        : 0;


            var model = new SaqueDosBotoes(valor, nota);

            TempData["SaldoAtualizado"] = model.Saldoatualizado;

            // Salvar o novo estado na sessão
            HttpContext.Session.SetInt32("Nota10", nota.Nota10 ?? 0);
            HttpContext.Session.SetInt32("Nota20", nota.Nota20 ?? 0);
            HttpContext.Session.SetInt32("Nota50", nota.Nota50 ?? 0);
            HttpContext.Session.SetInt32("Nota100", nota.Nota100 ?? 0);
            HttpContext.Session.SetInt32("Nota200", nota.Nota200 ?? 0);

            


            // Redireciona para a página de sucesso ou confirmação
            return View("menu/saque/saque",nota);
        }


    }


}


