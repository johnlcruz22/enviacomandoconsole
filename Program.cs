using System;
using System.Net.Sockets;
using System.Collections.Generic;
using Npgsql;
using System.Text;
using System.Xml;
using System.IO;



namespace Tipos
{
    
    class Program
    {
        

        static void Main(string[] args)
        {

            bool controle = true;
            int opcao;


            while (controle)
            {
               Console.WriteLine("\r\nDIGITE:\r\n 1- PARA VER COMANDOS \r\n 2- PARA INSERIR NOVOS COMANDOS \r\n 3- PARA ENVIAR 1 COMANDO \r\n 4- PARA SAIR \r\n ==================================");
                try
                {
                    opcao = Convert.ToInt16(Console.ReadLine());
                }catch(Exception ex)
                {
                    Console.WriteLine("DIGITE UM VALOR VALIDO ANIMAL, DEU ERRO >" + ex.Message);
                    opcao = 99;
                }
                 

                switch (opcao)
                {
                    case 1:
                        Lerxml();
                        break;
                    case 2:
                        Cadastrarcomando();
                        break;
                    case 3:
                        Enviacomando();
                        break;
                    case 4:
                        controle = false;
                        break;
                    default:
                        Console.WriteLine("DIGITE NOVAMENTE !");
                        break;

                }

            }

            /*            //if(a==10) Console.WriteLine(a);
                        //string v = a <= Convert.ToInt32(e) ? "imprimir A": "imprimir B";
            }*/

        }

        public static NpgsqlConnection Conectarpg(string ip,
                                          string porta,
                                          string usuario,
                                          string senha,
                                          string database)
        {
            NpgsqlConnection conn = new NpgsqlConnection(ip + ";" + porta + ";" + usuario + ";" + senha + ";" + database + ";");

            try
            {
                conn.Open();
                return conn;
            }
            catch (Exception erro)
            {
                Console.WriteLine("ERRO AO CONECTAR VERIFIQUE OS DADOS" + erro);
                return conn;
            }

        }

        public static List<string> Consultar_banco(NpgsqlConnection conexao, string consulta)
        {
            int i;
            List<string> comandos_disponiveis = new List<string>();
            //NpgsqlCommand cmd = new NpgsqlCommand("select distinct(logccomando_enviado) from log_comando where logceveprojeto=87;",conexao);
            NpgsqlCommand cmd = new NpgsqlCommand(consulta, conexao);
            try
            {
                NpgsqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    for (i = 0; i < rd.FieldCount; i++)
                    {
                        //Console.Write("{0} \t", rd[i]);
                        comandos_disponiveis.Add(rd[i].ToString());
                    }
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Erro " + e);
            }

            //conn.Close();
            return comandos_disponiveis;
        }



        public static void Xml_gravar(string chamada, string strcomando, string parametros)
        {

            try
            {
                if (File.Exists(Environment.CurrentDirectory + "\\config.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    // Create a new book element. 
                    doc.Load(Environment.CurrentDirectory + "\\config.xml");
                    Console.WriteLine(doc.InnerText);

                    
                    XmlNode novocomando = doc.CreateElement("COMANDOS");

                    XmlElement xmlchamada = doc.CreateElement("chamada");
                    xmlchamada.InnerText = chamada;
                    novocomando.AppendChild(xmlchamada);

                    XmlElement xmlcomando = doc.CreateElement("stringcomando");
                    xmlcomando.InnerText = strcomando;
                    novocomando.AppendChild(xmlcomando);

                    XmlElement xmlparametro = doc.CreateElement("parametros");
                    xmlparametro.InnerText = parametros;
                    novocomando.AppendChild(xmlparametro);

                    XmlNode nodoRaiz = doc.DocumentElement;
                    nodoRaiz.InsertAfter(novocomando, nodoRaiz.LastChild);


                    Console.WriteLine(doc.InnerText);

                    doc.Save(Environment.CurrentDirectory + "\\config.xml");
                    
                }
                else
                {
                    XmlTextWriter comandos = new XmlTextWriter(Environment.CurrentDirectory + "\\config.xml", Encoding.UTF8);
                    comandos.WriteStartDocument();

                    comandos.WriteStartElement("SERVIDORCOMANDOS");
                    comandos.WriteStartElement("COMANDOS");
                    comandos.WriteElementString("chamada", chamada.ToUpper());
                    comandos.WriteElementString("stringcomando", strcomando.ToUpper());
                    comandos.WriteElementString("parametros", parametros);
                    comandos.WriteEndElement();
                    comandos.WriteEndElement();
                    comandos.WriteEndDocument();

                    comandos.Close();
                    Console.WriteLine("XML CRIADO COM SUCESSO");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar xml de comandos " + ex.Message);
            }

        }

        public static void Lerxml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Environment.CurrentDirectory + "\\config.xml");

            XmlNodeList listadecomandos = doc.SelectNodes("SERVIDORCOMANDOS/COMANDOS");
            XmlNode comandosxml;

            for (int i=0; i<listadecomandos.Count; i++)
            {
                comandosxml = listadecomandos.Item(i);

                string comando = comandosxml.SelectSingleNode("stringcomando").InnerText;
                string qtdeparam = comandosxml.SelectSingleNode("parametros").InnerText;

                Console.Write("String do Comando: {0}, Qtde de Parametros: {1}, ID comando: {2}\n", comando,qtdeparam,i+1);


            }


        }

        public static bool Cadastrarcomando()
        {
            bool parar = true;

            while (parar)
            {

                Console.WriteLine("PARA SAIR DIGITE 'EXIT' A QUALQUER MOMENTO" + "\r\n ______");

                Console.WriteLine("Digite a chamada do comando");
                string chamada = Console.ReadLine();

                if (chamada.ToUpper().CompareTo("EXIT") == 0)
                {
                    parar = false;
                }

                Console.WriteLine("Digite a string do comando");
                string comando = Console.ReadLine();

                if (comando.ToUpper().CompareTo("EXIT") == 0)
                {
                    parar = false;
                }

                Console.WriteLine("Digite a quantidade de parametros");
                string parametro = Console.ReadLine();

                if (parametro.ToUpper().CompareTo("EXIT") == 0)
                {
                    parar = false;
                }

                if (chamada != null && comando != null && parametro != null &&
                    chamada.ToUpper().CompareTo("EXIT") != 0 && comando.ToUpper().CompareTo("EXIT") != 0 && parametro.ToUpper().CompareTo("EXIT") != 0)
                {
                    Xml_gravar(chamada, comando, parametro);
                }

            }
            return true;
        }

        public static string Enviacomando() 
        {
            XmlDocument doc   = new XmlDocument();
            TcpClient cliente = new TcpClient();
            List<string> listacomandosid = new List<string>();
            List<string> listaparametrosid = new List<string>();
            List<string> receberparamusu = new List<string>();
            string porta = "";


            doc.Load(Environment.CurrentDirectory + "\\config.xml");
            string linhacomandook = "";
            string retornodoservidor = "";
            string esn= "359856070260130";

            XmlNodeList listadecomandos = doc.SelectNodes("SERVIDORCOMANDOS/COMANDOS");
            XmlNode comandosxml;


            for (int i = 0; i < listadecomandos.Count; i++)
            {
                comandosxml = listadecomandos.Item(i);
                listacomandosid.Add(comandosxml.SelectSingleNode("stringcomando").InnerText);
                listaparametrosid.Add(comandosxml.SelectSingleNode("parametros").InnerText);
            }

            bool Controledoenvio = true;

            while (Controledoenvio)
            {
                Console.WriteLine("\r\nDIGITE O NUMERO DO ID PARA EXECUTAR O COMANDO! OU 999 PARA SAIR");
                
                int iddigitado = Convert.ToInt16(Console.ReadLine());

                if ((iddigitado < 0 || listadecomandos.Count < iddigitado) && iddigitado != 999)
                {
                    Console.WriteLine("\r\nID INVALIDO \r\n");
                }
                else if (iddigitado == 999)
                {
                    Controledoenvio = false;
                }
                else
                {
                    string strenviarcomando = listacomandosid[iddigitado - 1].ToUpper();
                    int lerparametros = Convert.ToInt16(listaparametrosid[iddigitado - 1]);

                    for (int j=0; j<lerparametros; j++)
                    {
                        Console.WriteLine("\r\nDIGITE O PARAMETRO NECESSARIO PARA ENVIAR");
                        receberparamusu.Add(Console.ReadLine().ToUpper());
                    }

                    if (receberparamusu.Count >= 1)
                    {
                        string todoparametro = "\r\n";
                        foreach(string paramtmp in receberparamusu)
                        {
                            todoparametro += paramtmp + "\r\n";
                        }

                        linhacomandook = esn + " 12133 0 1 0 " + strenviarcomando + todoparametro;
                        Controledoenvio = false;
                    }
                    else
                    {
						linhacomandook = esn+ " 12133 0 1 0 " + strenviarcomando + "\r\n\r\n ";
                        Controledoenvio = false;
                    }
                   
                }
            }

                Console.WriteLine("Digite o ip ou 'DESENV' para enviar para 10.1.110.20:22224");
                string ip = Console.ReadLine();
                

            if (ip.ToUpper().CompareTo("DESENV") != 0)
                {
                    Console.WriteLine("Digite a porta do quarto range");
                    porta = Console.ReadLine();
                }

            try
            {
                if (ip.ToUpper().CompareTo("DESENV") == 0)
                {
                    cliente.Connect("10.1.110.20", 22224);
                }
                else
                {
                    cliente.Connect(ip, Convert.ToInt32(porta));
                }

            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Argumento Nulo");
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Fora do Range");
                throw;
            }
            catch (SocketException)
            {
                Console.WriteLine("Nuloo");
                throw;
            }
            finally
            {
                bool controle = true;

                while (controle)
                {
                    Console.WriteLine("ENVIO DE COMANDOS PARA SERVIDOR " + ip + " PORTA " + porta.ToString());
                    Console.WriteLine("PARA SAIR DIGITE > EXIT");
                    Console.WriteLine("PARA ENVIAR COMANDO DIGITE 1");
                    string resposta = Console.ReadLine();

                    if (resposta == "1")
                    {

                        string V = linhacomandook;
                        byte[] comando;
                        byte[] retorno = new byte[28880];
                        
                        ASCIIEncoding encoding = new ASCIIEncoding();
                        comando = encoding.GetBytes(V);

                        try
                        {
                            NetworkStream Comunicacao = cliente.GetStream();
                            Console.WriteLine("Enviando comando " + V);
                            Comunicacao.Write(comando);

                            Int32 tamby  = Comunicacao.Read(retorno, 0, retorno.Length);
                            
                            retornodoservidor = encoding.GetString(retorno, 0, tamby);


                            Console.WriteLine(retornodoservidor);
                            Comunicacao.Close();
                            Console.WriteLine("saiu sem erros");
                            controle = false;

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else if (resposta.ToUpper().CompareTo("EXIT") == 0)
                    {
                        controle = false;
                    }

                }
                //if(a==10) Console.WriteLine(a);
                //string v = a <= Convert.ToInt32(e) ? "imprimir A": "imprimir B";
            }

            string retornofim = "OBRIGADO POR USAR O NOSSO SOFTWARE 8)";
            return retornofim;
        }

    }
}
