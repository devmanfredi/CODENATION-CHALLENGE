using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;
using System.Collections.Generic;

namespace CriptografiaJulioCesar
{
    class Program
    {
        //SNIPE 208
        static void Main(string[] args)
        {
            Requisição();
            Json challenge = LerObjetoDeArquivo();
            Descriptografa(challenge);
            ResumoSHA1(challenge);
            SalvarObjetoEmArquivo(challenge);
            //string json_challenge = "{" +
            //  " 'numero_casas':2, " +
            //  " 'token':'91e93d89f007589a868fabc78a7354db1b3564d0'," +
            //  " 'cifrado':'lwuv fq pqv etgcvg c hkng ecnngf -th. nctta ycnn'," +
            //  " 'decifrado':''," +
            //  " 'resumo_criptografico':'' " +
            //  "}";

            ////Json jsonModel = JsonConvert.DeserializeObject<Json>(json_challenge);
            //Descriptografa(jsonModel);
            //CalculateSHA1(jsonModel);
            //SalvarObjetoEmArquivo(jsonModel);
            ////CreatePerson(jsonModel);

            //ConverterObjetoParaJson(jsonModel);
            ////IMPRIMI O JSON SEM AQUELAS FORMATAÇÕES PADRÃO
            //Console.WriteLine($"{jsonModel.numero_casas}\n{jsonModel.token}\n{jsonModel.cifrado}\n{jsonModel.decifrado}\n{jsonModel.resumo_criptografico}");

            ////VERIFICAR SE ESTÁ PARECIDADA COM EXEMPLO
            //string teste_codenation = "a ligeira raposa marrom saltou sobre o cachorro cansado 1a.a";
            //Criptografa(teste_codenation,3);

            Console.WriteLine(challenge);
        }


        /// <summary>
        /// FAZ A REQUISIÇÃO E SALVA O ARQUIVO JSON
        /// </summary>
        static void Requisição()
        {
            var requisicaoWeb = WebRequest.CreateHttp("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            requisicaoWeb.Method = "GET";
            requisicaoWeb.UserAgent = "RequisicaoWebDemo";

            using (var resposta = requisicaoWeb.GetResponse())
            {
                var streamDados = resposta.GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);
                object objResponse = reader.ReadToEnd();

                //RETORNA UM MODELO PADRAO PARA SALVAR EM JSON
                object new_objResponse =  ConverteJsonParaObjeto(objResponse.ToString());
                
                //SALVA A REQUISIÇÃO EM UM ARQUIVO
                StreamWriter stream = new StreamWriter("C:\\CodeNationChallenge\\answer.json");
                JsonTextWriter writer = new JsonTextWriter(stream);
                JsonSerializer serializer = new JsonSerializer();
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, new_objResponse);
                stream.Close();

                streamDados.Close();
                resposta.Close();

            }
        }
        
        /// <summary>
        /// PEGA A REQUISIÇÃO E PASSA PARA UM MODEL(OBJETO)
        /// </summary>
        /// <param name="objResponse"></param>
        /// <returns></returns>
        static object ConverteJsonParaObjeto(string objResponse)
        {
            Json Model = JsonConvert.DeserializeObject<Json>(objResponse);
            return Model;
        }

        /// <summary>
        /// LER O ARQUIVO DA REQUISIÇÃO E RETORNA UM OBJETO PARA  DESCRIPTOGRAFAR
        /// </summary>
        /// <returns></returns>
        static Json LerObjetoDeArquivo()
        {
            StreamReader stream = new StreamReader("C:\\CodeNationChallenge\\answer.json");
            JsonTextReader reader = new JsonTextReader(stream);
            JsonSerializer serializer = new JsonSerializer();

            Json Model = serializer.Deserialize<Json>(reader);

            stream.Close();

            return Model;

        }

        /// <summary>
        /// RECEBE O MODEL E DESCRIPTOGRAFA O CAMPO CIFRADO
        /// </summary>
        /// <param name="Model"></param>
        static void Descriptografa(Json Model)
        {
            int cifra = Model.numero_casas;
            string des_crypt = "";
            foreach (char c in Model.cifrado)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    des_crypt += (char)(c);
                }
                else
                {
                    des_crypt += (char)(c - cifra);
                }
            }

            des_crypt = des_crypt.Replace("\u001e", " ");
            Model.decifrado = des_crypt.ToLower();

        }

        /// <summary>
        /// RECEBE O MODEL JÁ DESCRIPTOGRAFADO PARA FAZER O RESUMO CRIPTOGRÁFICO
        /// </summary>
        /// <param name="Resumo"></param>
        static Json ResumoSHA1(Json Model)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(Model.decifrado);
                System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                Model.resumo_criptografico = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }

            return Model;
        }

        /// <summary>
        /// AQUI VAI SALVAR O ARQUIVO JSON COMPLETO
        /// </summary>
        /// <param name="answer"></param>
        static void SalvarObjetoEmArquivo(Json Model)
        {
            StreamWriter stream = new StreamWriter("C:\\CodeNationChallenge\\answer.json");
            JsonTextWriter writer = new JsonTextWriter(stream);
            JsonSerializer serializer = new JsonSerializer();

            writer.Formatting = Formatting.Indented;

            serializer.Serialize(writer, Model);

            stream.Close();
        }



        #region CONVERSÃO JSON

        //static void ConverterJsonParaObjeto()
        //{
        //}
        static void ConverterObjetoParaJson(Json json_Model)
        {
            Json json_topico = new Json()
            {
                numero_casas = json_Model.numero_casas,
                token = json_Model.token,
                cifrado = json_Model.cifrado,
                decifrado = json_Model.decifrado,
                resumo_criptografico = json_Model.resumo_criptografico,
            };

            var json = JsonConvert.SerializeObject(json_topico);

            //IMPRIME O JSON DE FATO
            //ESTE QUE VAI PARA A API
            Console.WriteLine(json);

        }

        #endregion

        static void Criptografa(string mensagem, int cifra)
        {
            //CRIPTOGRAFIA

            string crypt = "";
            foreach (char c in mensagem)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    crypt += (char)(c);
                }
                else
                {
                    crypt += (char)(c + cifra);
                }
            }
            crypt = crypt.Replace("#", " ").ToLower();
            Console.WriteLine(crypt);
            DescriptografaString(crypt, 3);


        }


        //VERIFICAR SE ESTÁ PARECIDADA COM EXEMPLO
        static void DescriptografaString(string mensagem, int cifra)
        {
            string des_crypt = "";
            foreach (char c in mensagem)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    des_crypt += (char)(c);
                }
                else
                {
                    des_crypt += (char)(c - cifra);
                }
            }
            des_crypt = des_crypt.Replace("\u001d", " ");
            Console.WriteLine(des_crypt);
            //Console.WriteLine();

            //Console.WriteLine(des_crypt);

            //ConverterObjetoParaJson(crypt, des_crypt);
            //ConverterJsonParaObjeto(crypt, des_crypt);

            //Console.ReadLine();
        }

        // Método para criar a pessoa e enviar para a Web API


        static async void CreatePerson(Json Model)
        {

            // Cria a pessoa e armazena o resultado
            var isOK = await CreatePersonAsync(Model);

            // Verifica se obteve sucesso
            if (isOK)
            {
                // Pessoa criada com sucesso
            }
        }

        /// <summary>
        /// Cria uma pessoa
        /// </summary>
        /// <param name="person">Objeto 'Person'</param>
        /// <returns></returns>
        static async Task<bool> CreatePersonAsync(Json Model)
        {

            //File answer = "answer.json";

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Transforma o objeto em json
            string json = JsonConvert.SerializeObject(Model);

            // Envia o json para a API e verifica se obteve sucesso
            HttpResponseMessage response = await client.PostAsync("CreatePerson", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }

}


