using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriCoopMed.model
{
    class Paciente
    {
        private string nome;
        private string cartao_numero;
        private string segurador_empresa;

        public Paciente()
        {
            this.nome = "";
            this.cartao_numero = "";
            this.segurador_empresa = "";
        }
    

        public Paciente(string nome, string cartao_numero, string segurador_empresa)
        {
            this.nome = nome;
            this.cartao_numero = cartao_numero;
            this.segurador_empresa = segurador_empresa;
        }

        public string Segurador_empresa { get => segurador_empresa; set => segurador_empresa = value; }
        public string Cartao_numero { get => cartao_numero; set => cartao_numero = value; }
        public string Nome { get => nome; set => nome = value; }
    }
}
