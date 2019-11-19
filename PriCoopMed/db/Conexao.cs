using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PriCoopMed.db
{
    class Conexao
    {

        public SqlConnection conexao;
        string connetionString;

        public Conexao(string instancia, string basedados, string usuario, string senha)
        {
            this.connetionString = @"Data Source=" + instancia +";Initial Catalog=" + basedados + ";User ID=" + usuario + ";Password=" + senha + "!";
            conexao = new SqlConnection(this.connetionString);
        }

        public Conexao()
        {
            this.connetionString = @"Data Source=ACADV10\PRIMAVERA;Initial Catalog=PRICOOP;User ID=sa;Password=jmr2013!";
            conexao = new SqlConnection(connetionString);
        }

        public void abrir()
        {
            try
            {
                if ( this.conexao.State != System.Data.ConnectionState.Open)
                {
                    this.conexao.Open();
                }
            } catch ( Exception err )
            {
                MessageBox.Show("Ocorreu um erro na abertura da conexao ");

            }

        }

        public void fechar()
        {
            try
            {
                this.conexao.Close();

            }
            catch ( Exception err )
            {
                MessageBox.Show("Ocorreu um erro no encerramento da conexao\n" + err.Message);

            }
        }

        public SqlDataReader consulta ( string sql )
        {
            SqlCommand com;
            SqlConnection con = new SqlConnection(this.connetionString);
            SqlDataReader dataReader = null;
            com = new SqlCommand(sql, con);

            try
            {
                con.Open();
                dataReader =  com.ExecuteReader();
            }
            catch ( Exception err )
            {
                MessageBox.Show("Consulta: " + err.Message);
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            } 
            

            return dataReader;
        }

        //public SqlDataReader consultaAvancada(string sql)
        //{
        //    this.comando = new SqlCommand(sql, this.conexao);

        //    try
        //    {
        //        this.abrir();
        //        return this.comando.ExecuteReader();
        //    }
        //    catch (Exception err)
        //    {
        //        MessageBox.Show("Ocorreu um erro de consulta");
        //        MessageBox.Show(err.Message);
        //        if (this.conexao.State == System.Data.ConnectionState.Open)
        //        {
        //            this.fechar();
        //        }
        //    }

        //    return null;
        //}


    }
}
