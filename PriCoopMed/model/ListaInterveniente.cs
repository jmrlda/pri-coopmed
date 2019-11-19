using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriCoopMed.model
{
    class ListaInterveniente
    {


        private List<Interveniente> listaInterveniente;

        public ListaInterveniente()
        {
            listaInterveniente = new List<Interveniente>();
        }

        public ListaInterveniente(List<Interveniente> listaInterveniente)
        {
            this.listaInterveniente = listaInterveniente;
        }
    } 
}
