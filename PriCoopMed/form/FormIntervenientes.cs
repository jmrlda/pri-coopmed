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
using Primavera.Extensibility.Integration;

namespace PriCoopMed.form
{
    public partial class FormIntervenientes : Form
    {

       
        SqlDataReader objLista;
        Conexao con = new Conexao();
        public ListBox intervenientes_lista;

        public FormIntervenientes()
        {
            InitializeComponent();
            intervenientes_lista = new ListBox();
        }

        private void FormIntervenientes_Load(object sender, EventArgs e)
        {
            //lstboxTodosIntervenientes.Items.Add("teste");
            ////lstboxTodosIntervenientes.Items


            //lstboxTodosIntervenientes.Items.Add("teste1");
            //string cliente;
            //string SQL;
            //StdBE100.StdBELista objLista = new StdBE100.StdBELista();

            //ErpBS100.ErpBS engine = new ErpBS100.ErpBS();

            //cliente = "SEG002";

            //SQL = "SELECT * FROM clientes where CDU_Seguradora = '" + cliente + "'";
            //objLista = engine.Consulta(SQL);
            //while (!(objLista.NoInicio() || objLista.NoFim()))
            //{
            //    MessageBox.Show(objLista.Valor(1));

            //}

            string sql = null;
         

            sql = "select Vendedor, Nome, Comissao from Vendedores";
            objLista = con.consulta(sql);


            if (objLista == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("FormIntervenientes_Load: Ocorreu de consulta a base de dados");
                return;
            }

            while (objLista.Read())
            {
                this.lstboxTodosIntervenientes.Items.Add(objLista.GetValue(objLista.GetOrdinal("Vendedor")).ToString() + "\t  \t" +  objLista.GetValue(objLista.GetOrdinal("Nome")).ToString());

                //objLista.GetValue(objLista.GetOrdinal("Nome")).ToString()
                //objLista.GetValue(objLista.GetOrdinal("Comissao")).ToString()
                //objLista.GetValue(objLista.GetOrdinal("Vendedor")).ToString()

            }
            objLista.Close();
            con.fechar();



        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (this.lstboxTodosIntervenientes.SelectedItem is Object)
            {
                this.lstboxIntervenientesSelecionados.Items.Add(this.lstboxTodosIntervenientes.SelectedItem);
                this.lstboxTodosIntervenientes.Items.Remove(this.lstboxTodosIntervenientes.SelectedItem);

            } else
            {
                MessageBox.Show("Selecionar pelo menos um Interveniente!");
            }
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (this.lstboxIntervenientesSelecionados.SelectedItem is Object)
            {
                this.lstboxTodosIntervenientes.Items.Add(this.lstboxIntervenientesSelecionados.SelectedItem);
                this.lstboxIntervenientesSelecionados.Items.Remove(this.lstboxIntervenientesSelecionados.SelectedItem);

            }
            else
            {
                MessageBox.Show("Selecionar pelo menos um Interveniente!");
            }





        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (this.lstboxIntervenientesSelecionados.Items.Count <= 0)
            {
                MessageBox.Show(this, "Artigos possui  intervenientes\nPor favor Selecione pelo menos um!", "Atenção", MessageBoxButtons.OK);
            }

            this.intervenientes_lista = lstboxIntervenientesSelecionados;
            this.Close();
        }

        //private void getCliente()
        //{
        //    string sql = null;

        //    sql = "select Vendedor, Nome, Comissao from Vendedores";
        //    objLista = con.consulta(sql);


        //    if (objLista == null)
        //    {
        //        //Console.WriteLine("Ocorreu um erro", objLista);
        //        MessageBox.Show("Ocorreu de consulta a base de dados");
        //        return;
        //    }

        //    while (objLista.Read())
        //    {

        //        //objLista.GetValue(objLista.GetOrdinal("Nome")).ToString()
        //        //objLista.GetValue(objLista.GetOrdinal("Comissao")).ToString()
        //        //objLista.GetValue(objLista.GetOrdinal("Vendedor")).ToString()

        //    }
        //    objLista.Close();
        //    con.comando.Dispose();
        //    con.fechar();
        //}

        private void FormIntervenientes_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lstboxTodosIntervenientes_DoubleClick(object sender, EventArgs e)
        {
            if (this.lstboxTodosIntervenientes.SelectedItem is Object)
            {
                this.lstboxIntervenientesSelecionados.Items.Add(this.lstboxTodosIntervenientes.SelectedItem);
                this.lstboxTodosIntervenientes.Items.Remove(this.lstboxTodosIntervenientes.SelectedItem);

            }
            else
            {
                MessageBox.Show("Selecionar pelo menos um Interveniente!");
            }
        }

        private void lstboxIntervenientesSelecionados_DoubleClick(object sender, EventArgs e)
        {
            if (this.lstboxIntervenientesSelecionados.SelectedItem is Object)
            {
                this.lstboxTodosIntervenientes.Items.Add(this.lstboxIntervenientesSelecionados.SelectedItem);
                this.lstboxIntervenientesSelecionados.Items.Remove(this.lstboxIntervenientesSelecionados.SelectedItem);

            }
            else
            {
                MessageBox.Show("Selecionar item para remover!");
            }

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
