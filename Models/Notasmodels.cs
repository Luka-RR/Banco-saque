namespace Banco_ASP_2.Models
{


    //armazena as notas preenchidas nas caixa 
    public class Notasmodels
    {
        public int? Nota10 { get; set; }
        public int? Nota20 { get; set; }
        public int? Nota50 { get; set; }
        public int? Nota100 { get; set; }
        public int? Nota200 { get; set; }
        public decimal SaldoCalculado =>
        (Nota10 ?? 0) * 10 +
        (Nota20 ?? 0) * 20 +
        (Nota50 ?? 0) * 50 +
        (Nota100 ?? 0) * 100 +
        (Nota200 ?? 0) * 200;

        public decimal? Saldo { get; set; }






        public Dictionary<int, int> Notas => new Dictionary<int, int>
        {
            { 10, Nota10 ?? 0},
            { 20, Nota20 ?? 0},
            { 50, Nota50 ?? 0},
            { 100, Nota100 ?? 0 },
            { 200, Nota200 ?? 0}

        };


    }


    public class OutroValor
    {
        int? othervalue { get; set; }
    }




    public class SomaNotas

    {
        public decimal somanotas10 { get; set; }
        public decimal somanotas20 { get; set; }
        public decimal somanotas50 { get; set; }
        public decimal somanotas100 { get; set; }
        public decimal somanotas200 { get; set; }

        public decimal saldoBanco { get; set; }
        public SomaNotas(Notasmodels nota)
        {
            somanotas10 = 10 * (nota.Nota10 ?? 0);
            somanotas20 = 20 * (nota.Nota20 ?? 0);
            somanotas50 = 50 * (nota.Nota50 ?? 0);
            somanotas100 = 100 * (nota.Nota100 ?? 0);
            somanotas200 = 200 * (nota.Nota200 ?? 0);


            saldoBanco = somanotas10 + somanotas20 + somanotas50 + somanotas100 + somanotas200;


        }
    }





    public class SaqueDosBotoes
    {
        public int Valorbotao { get; set; }
        public decimal Saldoatualizado { get; set; }

        public SaqueDosBotoes(int valorbotoes, Notasmodels nota)
        {
            Valorbotao = valorbotoes;

            if (nota.Saldo.HasValue)
            {
                Saldoatualizado = nota.Saldo.Value - valorbotoes;
            }



        }

    }


    public class CalculoDoBanco
    {
        public int valorbotao2 { get; set; }
        public decimal saldoBancoAtualizado { get; set; }

        public bool OperacaoInvalida { get; set; }


        public CalculoDoBanco(int valorbotoes2, Notasmodels nota, decimal saldoBanco)
        {
            valorbotao2 = valorbotoes2;

            saldoBancoAtualizado = saldoBanco;

            OperacaoInvalida = true;


            saldoBancoAtualizado -= valorbotao2;


            int valorRestante = valorbotao2;

            while (valorRestante > 0)
            {
                if (valorRestante >= 200 && nota.Nota200 > 0)
                {
                    valorRestante -= 200;
                    nota.Nota200--;
                }
                else if (valorRestante >= 100 && nota.Nota100 > 0)
                {
                    valorRestante -= 100;
                    nota.Nota100--;
                }
                else if (valorRestante >= 50 && nota.Nota50 > 0)
                {
                    valorRestante -= 50;
                    nota.Nota50--;
                }
                else if (valorRestante >= 20 && nota.Nota20 > 0)
                {
                    valorRestante -= 20;
                    nota.Nota20--;
                }
                else if (valorRestante >= 10 && nota.Nota10 > 0)
                {
                    valorRestante -= 10;
                    nota.Nota10--;
                }
                else
                {
                    // Aqui entraria se NÃO for possível sacar exatamente
                    OperacaoInvalida = false;
                    break;
                }
            }


        }

    }
        // MODELS - CalculadoraDeNotas.cs
        public class CalculadoraDeNotas
        {
            public static List<Dictionary<int, int>> CalcularOpcoesDeSaque(
                decimal valor,
                Dictionary<int, int> notasDisponiveis)
            {
                var raw = new List<Dictionary<int, int>>();

                // 2️⃣ Gera as 3 heurísticas
                var ordemDesc = notasDisponiveis
                                    .OrderByDescending(n => n.Key)
                                    .ToDictionary(k => k.Key, v => v.Value);
                raw.Add(CalcularSaque(valor, ordemDesc));

                var ordemAsc = notasDisponiveis
                                    .OrderBy(n => n.Key)
                                    .ToDictionary(k => k.Key, v => v.Value);
                raw.Add(CalcularSaque(valor, ordemAsc));

                var ordemMista = new Dictionary<int, int>
            {
                {100, notasDisponiveis.GetValueOrDefault(100)},
                {50,  notasDisponiveis.GetValueOrDefault(50)},
                {200, notasDisponiveis.GetValueOrDefault(200)},
                {20,  notasDisponiveis.GetValueOrDefault(20)},
                {10,  notasDisponiveis.GetValueOrDefault(10)},
            };
                raw.Add(CalcularSaque(valor, ordemMista));

                // 2️⃣ Filtra vazios e duplicados
                var comparer = new NotaDictionaryComparer();
                var unicos = raw
                    .Where(dic => dic.Count > 0)
                    .Distinct(comparer)
                    .ToList();

                return unicos;
            }

            private static Dictionary<int, int> CalcularSaque(
                decimal valor,
                Dictionary<int, int> notas)
            {
                var resultado = new Dictionary<int, int>();
                decimal restante = valor;

                foreach (var nota in notas)
                {
                    int quantidade = (int)(restante / nota.Key);
                    int usar = Math.Min(quantidade, nota.Value);
                    if (usar > 0)
                    {
                        resultado[nota.Key] = usar;
                        restante -= usar * nota.Key;
                    }
                }

                return restante == 0
                    ? resultado
                    : new Dictionary<int, int>();
            }
        }



    public class NotaDictionaryComparer : IEqualityComparer<Dictionary<int, int>>
    {
        public bool Equals(Dictionary<int, int> a, Dictionary<int, int> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bv) || bv != kv.Value)
                    return false;
            }
            return true;
        }

        public int GetHashCode(Dictionary<int, int> obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (var kv in obj.OrderBy(x => x.Key))
                {
                    hash = hash * 23 + kv.Key.GetHashCode();
                    hash = hash * 23 + kv.Value.GetHashCode();
                }
                return hash;
            }
        }
    }



}





