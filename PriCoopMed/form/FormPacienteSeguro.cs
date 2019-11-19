using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PriCoopMed.db;
using PriCoopMed.vendas;
using Primavera.Extensibility.Integration;
using StdBE100;

namespace PriCoopMed.form
{
    public partial class FormPacienteSeguro : Form
    {

        string venda_cliente = "";
        SqlDataReader objLista;

        Conexao con ;
        string cliente;
        string SQL;

        public string cdu_paciente;
        public string cdu_seguro;
        public string cdu_nrCartao;
        public string cdu_autorizado_por;
        public string cdu_autorizacao_ref;
        Vendas editorVendas;
        public FormPacienteSeguro()
        {
            InitializeComponent();
            con = new Conexao();
            cdu_paciente        = null;
            cdu_seguro          = null;
            cdu_nrCartao        = null;
            cdu_autorizado_por  = null;
            cdu_autorizacao_ref = null;
        }
        public FormPacienteSeguro(String cliente, Vendas editorVendas)
        {
            venda_cliente = cliente;
            InitializeComponent();
            con = new Conexao();
            cdu_paciente = null;
            cdu_seguro = null;
            cdu_nrCartao = null;
            cdu_autorizado_por = null;
            cdu_autorizacao_ref = null;
            this.editorVendas = editorVendas;

        }


        private void FormPacienteSeguro_Load(object sender, EventArgs e)
        {
            con.abrir();

            getCliente();

        }




        private void btnLimpar_Click(object sender, EventArgs e)
        {
            this.txtAutorizadoPor.Text = "";
            this.txtCartaoNumero.Text = "";
            this.txtCartaoNumero.Text = "";
            this.txtReferenciaAutorizacao.Text = "";
            this.txtSeguradoraEmpresa.Text = "";
            this.cboPaciente.Text = "";
        }

        private void fetch_cliente(String cliente )
        {

        }

        private void getDadoCliente( string cliente )
        {
            SQL = "SELECT * FROM clientes where nome = '" + cliente + "'";
            objLista = con.consulta(SQL);
            if (objLista == null)
            {
                Console.WriteLine("getDadoCliente: Ocorreu um erro", objLista);
                return;
            }

            if (objLista.Read())
            {
                this.txtCartaoNumero.Text = objLista.GetValue(objLista.GetOrdinal("CDU_NrCartao")).ToString();
                this.txtSeguradoraEmpresa.Text = objLista.GetValue(objLista.GetOrdinal("CDU_seguradora")).ToString();

            }
            objLista.Close();
            con.fechar();
        }

        private string getNomeCliente()
        {
            SQL = "SELECT Nome FROM clientes where Cliente = '" + venda_cliente + "'";
            objLista = con.consulta(SQL);
            string nome_cliente = null;
            if (objLista == null)
            {
                Console.WriteLine("Ocorreu um erro", objLista);
                return null;
            }

            if (objLista.Read())
            {
                 nome_cliente  = objLista.GetValue(objLista.GetOrdinal("Nome")).ToString();

            }
            objLista.Close();
            con.fechar();

            return nome_cliente;
        }

        private void getCliente()
        {
            SQL = "SELECT * FROM clientes where CDU_Seguradora = '" + getNomeCliente() + "'";
            objLista = con.consulta(SQL);
            if (objLista == null)
            {
                Console.WriteLine("Ocorreu um erro", objLista);
                return;
            }

            int count = 0;
            while (objLista.Read())
            {
                this.cboPaciente.Items.Add(objLista.GetValue(objLista.GetOrdinal("Nome")));
                count++;
            }
            if ( count == 0 )
            {
                MessageBox.Show("Nenhum paciente encontrado para Seguradora/Empresa " + venda_cliente);
            }
            objLista.Close();
            con.fechar();

            ;
        }

        private void cboPaciente_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.getDadoCliente(this.cboPaciente.Text); 
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            this.cdu_paciente = this.cboPaciente.Text;
            this.cdu_seguro = this.txtSeguradoraEmpresa.Text;
            this.cdu_nrCartao = this.txtCartaoNumero.Text;
            this.cdu_autorizado_por = this.txtAutorizadoPor.Text;
            this.cdu_autorizacao_ref = this.txtReferenciaAutorizacao.Text;

            this.Close();
        }

        private void limpar( )
        {
            this.txtAutorizadoPor.Text = "";
            this.txtCartaoNumero.Text = "";
            this.txtCartaoNumero.Text = "";
            this.txtReferenciaAutorizacao.Text = "";
            this.txtSeguradoraEmpresa.Text = "";
            this.cboPaciente.Text = "";
        }

        private void teste ()
        {
            
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
