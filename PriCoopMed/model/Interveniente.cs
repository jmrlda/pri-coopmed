using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriCoopMed.model
{
    class Interveniente
    {
        private string vendedor;
        private string nome;
        private int comissao;

        public Interveniente()
        {
            this.vendedor = "";
            this.nome = "";
            this.comissao = 0;
        }


        public Interveniente(string vendedor, string nome, int comissao)
        {
            this.vendedor = vendedor;
            this.nome = nome;
            this.comissao = comissao;
        }

        public string Nome { get => nome; set => nome = value; }
        public string Vendedor { get => vendedor; set => vendedor = value; }
        public int Comissao { get => comissao; set => comissao = value; }
    }
}
