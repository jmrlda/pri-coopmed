using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;
using Primavera.Extensibility.Sales.Editors;
using Primavera.Extensibility.Integration.Modules.Base;
using PriCoopMed.model;
using PriCoopMed.form;
using System.Windows.Forms;
using PriCoopMed.db;
using System.Data.SqlClient;
using VndBE100;
using StdBE100;

namespace PriCoopMed.vendas
{
    public class Vendas : EditorVendas
    {
        //private int total_interveniente;
        private ListaInterveniente todos_interveniente = new ListaInterveniente();
        private Interveniente interveniente = new Interveniente();
        //private string artigoPrincipal;
        //private string interveniente_str;



        private static string SQL;
        public string cliente, artigo_principal;
        private int 
            artigo_principal_linha = -1,
            artigo_principal_total_conponentes = 0;

        //private static StdBE100.StdBELista objLista = new StdBE100.StdBELista();
        SqlDataReader objLista;

        Conexao con = new Conexao();
        FormPacienteSeguro paciente_form;
        FormIntervenientes interveniente_form;

        public string cdu_paciente        = null;
        public string cdu_seguro          = null;
        public string cdu_nrCartao        = null;
        public string cdu_autorizado_por  = null;
        public string cdu_autorizacao_ref = null;

        public override void ClienteIdentificado(string Cliente, ref bool Cancel, ExtensibilityEventArgs e)
        {
            con.abrir();

            this.cliente = Cliente;
            bool pass = false;
            base.ClienteIdentificado(Cliente, ref Cancel, e);
            SQL = "SELECT * FROM clientes where Cliente = '" + this.cliente + "'";
            objLista = con.consulta(SQL);


            if (objLista == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("ClienteIdentificado: Object reader nulo");
                return;
            }

            if (objLista.Read())
            {
                //Console.WriteLine(objLista.GetValue(objLista.GetOrdinal("TipoTerceiro")) + " - " + objLista.GetValue(objLista.GetOrdinal("CDU_TipoSeg")));
                if (objLista.GetValue(objLista.GetOrdinal("TipoTerceiro")).ToString() == "SEG" 
                    || 
                    objLista.GetValue(objLista.GetOrdinal("TipoTerceiro")).ToString() == "EMP")
                {

                    pass = true;
             
                    //this.DocumentoVenda.CamposUtil["CDU_SegEmp"].Valor = "teste";
                }
            }


            objLista.Close();
            con.fechar();

            if ( pass = true)
            {
                paciente_form = new FormPacienteSeguro(this.cliente, this);
                paciente_form.ShowDialog();
                campoUtilizador(paciente_form.cdu_paciente, paciente_form.cdu_seguro, paciente_form.cdu_nrCartao, paciente_form.cdu_autorizado_por, paciente_form.cdu_autorizacao_ref);

            }

            //StdBECampo teste = new StdBECampo();
            //teste.Nome = "CDU_Paciente";
            //teste.Valor = paciente_form.cdu_paciente;

            //this.DocumentoVenda.CamposUtil.Insere(teste);





        }

    
        public override void ArtigoIdentificado(string Artigo, int NumLinha, ref bool Cancel, ExtensibilityEventArgs e)
        {

            string descricao = null;
            int sqlCount = 0;
            int idx = 0;
            string sql = null;
            SqlDataReader obj;

            descricao = this.DocumentoVenda.Linhas.GetEdita(NumLinha).Descricao;
            sql = "SELECT * FROM ComponentesArtigos where ArtigoComposto = '" + Artigo + "' and CDU_Interv = 1";
            obj = con.consulta(sql);
            

            if (obj == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("ArtigoIdentificado: Object reader nulo");
                return;
            }

            bool pass = false;
            if (obj.Read())
            {
                pass = true;
            }


           

            if (pass == true)
            {
                interveniente_form = new FormIntervenientes();
                interveniente_form.ShowDialog();

                string thisArtigo = null;
                int linha_artigo_principal = -1;
                object artigo_obj = new object();
                VndBELinhaDocumentoVenda linha_artigo;
                 descricao = null;

                Interveniente interveniente = new Interveniente();
                if (interveniente_form.intervenientes_lista.Items.Count > 0)
                {
                    thisArtigo = obj.GetValue(obj.GetOrdinal("Componente")).ToString();
                    linha_artigo_principal = NumLinha + 1;
                    this.artigo_principal = thisArtigo;
                    obj.Close();
                    con.fechar();
                    foreach (string interv_ in interveniente_form.intervenientes_lista.Items)
                    {
                        linha_artigo = new VndBELinhaDocumentoVenda();
                        string interv = interv_.Split(' ')[0].ToString().Trim();
                        descricao = getVendedor(interv);
                 


                        //artigo_obj.artigo = thisArtigo;
                        linha_artigo.Artigo = thisArtigo;
                        linha_artigo.Quantidade = 0;
                        linha_artigo.Vendedor = interv;
                        linha_artigo.Descricao = descricao;

                        linha_artigo.TipoLinha = this.DocumentoVenda.Linhas.GetEdita(NumLinha).TipoLinha;
                        this.DocumentoVenda.Linhas.Insere(linha_artigo);

                    }

                    this.artigo_principal_total_conponentes = this.get_total_componente_artigo(Artigo);
                    this.artigo_principal_linha = NumLinha + 1;
                }
            }



  

            base.ArtigoIdentificado(Artigo, NumLinha, ref Cancel, e);
        }


        public override void DepoisDeGravar(string Filial, string Tipo, string Serie, int NumDoc, ExtensibilityEventArgs e)
        {
            insertComissao();
            base.DepoisDeGravar(Filial, Tipo, Serie, NumDoc, e);
        }


        public override void ValidaLinha(int NumLinha, ExtensibilityEventArgs e)
        {
            if (this.DocumentoVenda.Linhas.GetEdita(NumLinha).Artigo == this.artigo_principal )
            {
                if ( interveniente_form.intervenientes_lista.Items.Count > 0)
                {
                    string artigo, descricao, interveniente, familia;
                    int index,
                        total_intervenientes = interveniente_form.intervenientes_lista.Items.Count;
                    VndBELinhaDocumentoVenda linha_artigo;
                    //MessageBox.Show("total " + this.artigo_principal_total_conponentes.ToString());
                    

                    artigo = this.DocumentoVenda.Linhas.GetEdita(NumLinha).Artigo;
                    descricao = this.DocumentoVenda.Linhas.GetEdita(NumLinha).Descricao;
                    interveniente = interveniente_form.intervenientes_lista.Items[0].ToString().Trim();
                    familia = getFamilia(artigo);

                    linha_artigo = this.DocumentoVenda.Linhas.GetEdita(NumLinha);

                    for ( index = 2; index < total_intervenientes +  2 ; index++ )
                    {


                        //this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index + 1).Armazem = linha_artigo.Armazem;
                        //this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index + 1).Localizacao = linha_artigo.Localizacao;
                        //this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index + 1).Lote = linha_artigo.Lote;
                        //this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index + 1).CodIva = linha_artigo.CodIva;


                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).AlternativaGPR = linha_artigo.AlternativaGPR;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).AnaliticaCBL = linha_artigo.AnaliticaCBL;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).Armazem = linha_artigo.Armazem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).ArredFConv = linha_artigo.ArredFConv;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).AutoID = linha_artigo.AutoID;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).B2BNumLinhaOrig = linha_artigo.B2BNumLinhaOrig;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).BaseCalculoIncidencia = linha_artigo.BaseCalculoIncidencia;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).BaseIncidencia = linha_artigo.BaseIncidencia;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).Categoria = linha_artigo.Categoria;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).CCustoCBL = linha_artigo.CCustoCBL;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).ClasseActividade = linha_artigo.ClasseActividade;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).CodigoBarras = linha_artigo.CodigoBarras;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).CodIva = linha_artigo.CodIva;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).CodIvaEcotaxa = linha_artigo.CodIvaEcotaxa;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).CodIvaIEC = linha_artigo.CodIvaIEC;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Comissao = linha_artigo.Comissao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index ).ContaCBL = linha_artigo.ContaCBL;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index  ).CustoPrevisto = linha_artigo.CustoPrevisto;


                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).DataEntrega = linha_artigo.DataEntrega;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).DataStock = linha_artigo.DataStock;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Desconto1 = linha_artigo.Desconto1;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Desconto2 = linha_artigo.Desconto2;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Desconto3 = linha_artigo.Desconto3;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).DescontoComercial = linha_artigo.DescontoComercial;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Devolucao = linha_artigo.Devolucao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).DifPCMedio = linha_artigo.DifPCMedio;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Ecotaxa = linha_artigo.Ecotaxa;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Estado = linha_artigo.Estado;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).EstadoAdiantamento = linha_artigo.EstadoAdiantamento;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).EstadoBD = linha_artigo.EstadoBD;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).EstadoBE = linha_artigo.EstadoBE;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).EstadoOrigem = linha_artigo.EstadoOrigem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).EstadoPendente = linha_artigo.EstadoPendente;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).FactorConv = linha_artigo.FactorConv;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Fechado = linha_artigo.Fechado;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Formula = linha_artigo.Formula;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).FuncionalCBL = linha_artigo.FuncionalCBL;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IdAgendamento = linha_artigo.IdAgendamento;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IDB2BLinhaOrig = linha_artigo.IDB2BLinhaOrig;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IdContrato = linha_artigo.IdContrato;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IDHistorico = linha_artigo.IDHistorico;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IdLinhaEstorno = linha_artigo.IdLinhaEstorno;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IdLinhaOrigemCopia = linha_artigo.IdLinhaOrigemCopia;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IDLinhaOriginal = linha_artigo.IDLinhaOriginal;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IdLinhaPai = linha_artigo.IdLinhaPai;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IDObra = linha_artigo.IDObra;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IntrastatCodigoPautal = linha_artigo.IntrastatCodigoPautal;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IntrastatMassaLiq = linha_artigo.IntrastatMassaLiq;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IntrastatPaisOrigem = linha_artigo.IntrastatPaisOrigem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IntrastatRegiao = linha_artigo.IntrastatRegiao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IntrastatValorLiq = linha_artigo.IntrastatValorLiq;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ItemCod = linha_artigo.ItemCod;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ItemDesc = linha_artigo.ItemDesc;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IvaNaoDedutivel = linha_artigo.IvaNaoDedutivel;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IvaRegraCalculo = linha_artigo.IvaRegraCalculo;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).IvaValorDesconto = linha_artigo.IvaValorDesconto;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Localizacao = linha_artigo.Localizacao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Lote = linha_artigo.Lote;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Margem = linha_artigo.Margem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ModuloOrigemCopia = linha_artigo.ModuloOrigemCopia;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).MotivoEstorno = linha_artigo.MotivoEstorno;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).MovStock = linha_artigo.MovStock;

                        if ( linha_artigo.PrecoLiquido > 0 )
                        {
                            this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PrecUnit = linha_artigo.PrecoLiquido;

                        } else
                        {
                            this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PrecUnit = linha_artigo.PrecUnit;

                        }

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).NumLinDocOriginal = linha_artigo.NumLinDocOriginal;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).NumLinhaStkGerada = linha_artigo.NumLinhaStkGerada;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PCM = linha_artigo.PCM;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PCMDevolucao = linha_artigo.PCMDevolucao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PercentagemMargem = linha_artigo.PercentagemMargem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PercIncidenciaIVA = linha_artigo.PercIncidenciaIVA;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PercIvaDedutivel = linha_artigo.PercIvaDedutivel;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).PrecoLiquido = linha_artigo.PrecoLiquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ProcessoID = linha_artigo.ProcessoID;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).QuantCopiada = linha_artigo.QuantCopiada;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).QuantFormula = linha_artigo.QuantFormula;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).QuantReservada = linha_artigo.QuantReservada;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).QuantSatisfeita = linha_artigo.QuantSatisfeita;


                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).RegimeIva = linha_artigo.RegimeIva;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).RegraCalculoIncidencia = linha_artigo.RegraCalculoIncidencia;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).StockActual = linha_artigo.StockActual;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).SubEmpreitada = linha_artigo.SubEmpreitada;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).SujeitoRetencao = linha_artigo.SujeitoRetencao;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TaxaIva = linha_artigo.TaxaIva;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TaxaIvaEcotaxa = linha_artigo.TaxaIvaEcotaxa;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TaxaIvaIEC = linha_artigo.TaxaIvaIEC;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TaxaRecargo = linha_artigo.TaxaRecargo;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TipoAuto = linha_artigo.TipoAuto;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TipoCustoPrevisto = linha_artigo.TipoCustoPrevisto;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TipoLinha = linha_artigo.TipoLinha;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TipoOperacao = linha_artigo.TipoOperacao;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalDA = linha_artigo.TotalDA;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalDC = linha_artigo.TotalDC;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalDF = linha_artigo.TotalDF;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalEcotaxa = linha_artigo.TotalEcotaxa;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIEC = linha_artigo.TotalIEC;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIva = linha_artigo.TotalIva;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalRecargo = linha_artigo.TotalRecargo;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Unidade = linha_artigo.Unidade;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ValorIEC = linha_artigo.ValorIEC;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).ValorLiquidoDesconto = linha_artigo.ValorLiquidoDesconto;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).VariavelA = linha_artigo.VariavelA;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).VariavelB = linha_artigo.VariavelB;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).VariavelC = linha_artigo.VariavelC;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).WBSItem = linha_artigo.WBSItem;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).CamposUtil["CDU_pliquido"].Valor = this.DocumentoVenda.Linhas.GetEdita(NumLinha).PrecoLiquido;
                        this.DocumentoVenda.Linhas.GetEdita(NumLinha).CamposUtil["CDU_pliquido"].Valor = this.DocumentoVenda.Linhas.GetEdita(NumLinha).PrecoLiquido;

                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).TotalIliquido = linha_artigo.TotalIliquido;

                        string vendedor = null;

                        vendedor = this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Vendedor;
                        this.DocumentoVenda.Linhas.GetEdita(this.artigo_principal_total_conponentes + index).Comissao = getComissao(vendedor, familia);
                        this.DocumentoVenda.Linhas.GetEdita(NumLinha).CamposUtil["CDU_Vcomissao"].Valor = (getComissao(vendedor, familia) / 100) * this.DocumentoVenda.Linhas.GetEdita(NumLinha).PrecoLiquido;


                    }


                }
            }

                base.ValidaLinha(NumLinha, e);
        }




        public static string getCliente()
        {
            return "";
            
        }




        public int getComissao( string vendedor, string familia) 
        {

            int comissao = 0;
            string sql = null;

            sql = "SELECT * FROM comissoes where campo1 = '" + vendedor + "' and campo2 = '" + familia + "'";
            objLista = con.consulta(sql);


            if (objLista == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("getComissao: Object reader nulo");
                return -1;
            }

            if (objLista.Read())
            {
                comissao = int.Parse( objLista.GetValue(objLista.GetOrdinal("comissao")).ToString());
            }
            objLista.Close();
            con.fechar();

            return comissao;
        }




        public string getFamilia(string artigo)
        {

            string familia = null;
            string sql = null;

            sql = "SELECT Familia FROM Artigo where artigo = '" + artigo + "'";
            objLista = con.consulta(sql);


            if (objLista == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("getFamilia: Object reader nulo");
                return null;
            }

            if (objLista.Read())
            {
                familia =objLista.GetValue(objLista.GetOrdinal("familia")).ToString();
            }
            objLista.Close();
            con.fechar();

            return familia;
        }




        public string getEspecialidade(string especialidade)
        {

            string especialidade_descricao = null;
            string sql = null;

            if ( especialidade.Length > 0)
            {
                sql = "SELECT * FROM TDU_Especialidades where CDU_Codigo = '" + especialidade + "'";
                objLista = con.consulta(sql);


                if (objLista == null)
                {
                    //Console.WriteLine("Ocorreu um erro", objLista);
                    MessageBox.Show("getEspecialidade: Object reader nulo");
                    return null;
                }

                if (objLista.Read())
                {
                    especialidade_descricao = " - " + objLista.GetValue(objLista.GetOrdinal("CDU_descricao")).ToString();
                }

                //vendedor_descricao = nome + " " + getC
                objLista.Close();
                con.fechar();

            }


            return especialidade_descricao;
        }



        public string getClasse(string classe)
        {

            string classe_descricao = null;
            string sql = null;

            if (classe.Length > 0)
            {
                sql = "SELECT * FROM TDU_classes where CDU_Codigo = '" + classe + "'";
                objLista = con.consulta(sql);


                if (objLista == null)
                {
                    //Console.WriteLine("Ocorreu um erro", objLista);
                    MessageBox.Show("getClasse: Object reader nulo");
                    return null;
                }

                if (objLista.Read())
                {
                    classe_descricao = " - " + objLista.GetValue(objLista.GetOrdinal("CDU_descricao")).ToString();
                }

                //vendedor_descricao = nome + " " + getC
                objLista.Close();
                con.fechar();

            }


            return classe_descricao;
        }




        public string getVendedor(string codVendedor)
        {

            string nome = null;
            string especialidade = null;
            string classe = null;
            string vendedor_descricao = null;
            SqlDataReader obj;

            string sql = null;

            sql = "SELECT * FROM Vendedores where Vendedor = '" + codVendedor + "'";
            obj = con.consulta(sql);


            if (obj == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("getVendedor: Object reader nulo");
                return null;
            }

            if (obj.Read())
            {
                nome = obj.GetValue(obj.GetOrdinal("nome")).ToString();
                especialidade = obj.GetValue(obj.GetOrdinal("CDU_especialidade")).ToString();
                classe = obj.GetValue(obj.GetOrdinal("CDU_classe")).ToString();
                vendedor_descricao = nome + " " + getClasse(classe) + " " + getEspecialidade(especialidade);
                obj.Close();
                return vendedor_descricao;


            }


            //con.comando.Dispose();
            //con.fechar();
            return null;
        }


        public string insertComissao()
        {

            string classe_descricao = null;
            string sql = null;
            SqlDataReader obj;
                sql = "SELECT * FROM TDU_classes where CDU_Codigo ";
            obj = con.consulta(sql);


                if (obj == null)
                {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("insertComissao: Object reader nulo");
                return null;
                }

                if (obj.Read())
                {
                    classe_descricao = " - " + obj.GetValue(obj.GetOrdinal("CDU_descricao")).ToString();
                }

            //vendedor_descricao = nome + " " + getC
                obj.Close();

            con.fechar();

            


            return classe_descricao;
        }



        public void campoUtilizador(string paciente , string seguro, string nrCartao, string autorizado_por, string autorizado_ref)
        {


            DocumentoVenda.CamposUtil["CDU_Paciente"].Valor = paciente;
            DocumentoVenda.CamposUtil["CDU_SegEmp"].Valor = seguro;
            DocumentoVenda.CamposUtil["CDU_NrCartao"].Valor = nrCartao;
            DocumentoVenda.CamposUtil["CDU_Autorizado"].Valor = autorizado_por;
            DocumentoVenda.CamposUtil["CDU_RefAutoriz"].Valor = autorizado_ref;


        }

        void inserirInterveniente ( int numLinha )
        {

            //MessageBox.Show("inserir interveniente");
            //string thisArtigo = null;
            //int linha_artigo_principal = -1;
            //object artigo_obj = new object();
            //VndBELinhaDocumentoVenda linha_artigo = new VndBELinhaDocumentoVenda();

            //Interveniente interveniente = new Interveniente();
            //if ( interveniente_form.intervenientes_lista.Items.Count > 0)
            //{
            //    thisArtigo = this.objLista.GetValue(objLista.GetOrdinal("Componente")).ToString();
            //    linha_artigo_principal = numLinha + 1;


            //    foreach (string interv in interveniente_form.intervenientes_lista.Items)
            //    {

            //        //artigo_obj.artigo = thisArtigo;
            //        linha_artigo.Artigo = thisArtigo;
            //        linha_artigo.Quantidade = 0;
            //        linha_artigo.Vendedor = interv;
            //        linha_artigo.Descricao = getVendedor(interv);
            //        linha_artigo.TipoLinha = this.DocumentoVenda.Linhas.GetEdita(numLinha).TipoLinha;
            //        this.DocumentoVenda.Linhas.Insere(linha_artigo);
            //    }

            //}





        }

        int get_total_componente_artigo (string artigo)
        {
            int total = 0;
            string sql = "Select count(*) as total from ComponentesArtigos where ArtigoComposto = '" + artigo + "' ";
            objLista = con.consulta(sql);


            if (objLista == null)
            {
                //Console.WriteLine("Ocorreu um erro", objLista);
                MessageBox.Show("get_total_componente_artigo: Object reader nulo");
                return -1;
            }

            if (objLista.Read())
            {
                total = int.Parse( objLista.GetValue(objLista.GetOrdinal("total")).ToString());
            }

            //vendedor_descricao = nome + " " + getC
            objLista.Close();
            con.fechar();


            return total;
        }

    }
}
